#!/usr/bin/env bash
# pickle-run.sh <image-ref> <mods-dir> <config-dir> <report-dir>
# Runs the Cosmere Pickle suite headless in the RimWorld game image. summary.json is the
# verdict, never the exit code: Pickle exits 0 when zero scenarios ran, and xvfb-run
# teardown can fail a run whose scenarios all passed.
# Exit 0 pass, 1 failure, 75 no report plus a dead X server, which is worth a retry.
set -uo pipefail

IMAGE="${1:?usage: pickle-run.sh <image-ref> <mods-dir> <config-dir> <report-dir>}"
MODS_DIR="${2:?mods dir}"
CONFIG_DIR="${3:?config dir}"
REPORT_DIR="${4:?report dir}"

# Pickle runs every mod that ships a Pickle/ directory, and rimworks.pickle ships 26
# features of its own, so an unfiltered run spends 15 minutes on other people's tests.
SUITE_FILTER="${SUITE_FILTER:-Cosmere - Core}"

# Minutes. Pickle's own watchdog, which has to trip before any outer timeout does.
RUN_TIMEOUT="${PICKLE_RUN_TIMEOUT:-60}"

CONTAINER="${PICKLE_CONTAINER:-cosmere-pickle-suite}"
REPO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CFG="/home/app/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Config"
TMP="${RUNNER_TEMP:-/tmp}"

abs() { (cd "$1" 2>/dev/null && pwd) || { echo "error: no directory $1" >&2; exit 1; }; }

MODS_DIR="$(abs "$MODS_DIR")" || exit 1
CONFIG_DIR="$(abs "$CONFIG_DIR")" || exit 1
mkdir -p "$REPORT_DIR"
REPORT_DIR="$(abs "$REPORT_DIR")" || exit 1
chmod 777 "$REPORT_DIR"

# <mods-dir> holds the staged third party mods; the Cosmere three come off this checkout.
# rimworks.quickstarts stays with staging: it owns ModsConfig.xml, and an unlisted mod never loads.
mounts=(-v "$MODS_DIR:/game/Mods:ro" -v "$CONFIG_DIR:$CFG" -v "$REPORT_DIR:/out")
for pair in CosmereCore:Cosmere.Core CosmereRoshar:Cosmere.Roshar CosmereScadrial:Cosmere.Scadrial; do
  src="$REPO/${pair%%:*}"
  [[ -d "$src" ]] || { echo "error: no mod directory at $src" >&2; exit 1; }
  # runc cannot create a mountpoint inside a read-only bind, so /game/Mods needs the
  # directory to already be there.
  mkdir -p "$MODS_DIR/${pair##*:}"
  mounts+=(-v "$src:/game/Mods/${pair##*:}:ro")
done

echo "pulling $IMAGE ..."
start=$SECONDS
# A local image is fine when the pull fails: that is how this runs off a laptop.
if docker pull -q "$IMAGE"; then
  echo "pulled in $((SECONDS - start))s"
elif docker image inspect "$IMAGE" >/dev/null 2>&1; then
  echo "pull failed, using the local copy of $IMAGE"
else
  echo "error: cannot pull $IMAGE and there is no local copy" >&2
  exit 1
fi

# The verdict has to come from THIS run. Globbing for summary.json happily finds the one
# an earlier run left in the same directory.
stamp="$(mktemp)"
# A per run path, so a leftover tail from an older run cannot spill into this one's output.
clog="$(mktemp "$TMP/pickle-container.XXXXXX.log")"
trap 'rm -f "$stamp" "$clog"' EXIT

echo "running: -pickle-run=$SUITE_FILTER"
docker run --rm --name "$CONTAINER" "${mounts[@]}" "$IMAGE" \
  run-headless /game/RimWorldLinux \
    "-pickle-run=$SUITE_FILTER" \
    -pickle-report-dir=/out \
    -pickle-run-timeout="$RUN_TIMEOUT" \
    -pickle-max-film-seconds=0 \
    -pickle-no-http -pickle-no-browser \
    -logfile /out/Player.log \
  > "$clog" 2>&1 &
game=$!

tail -n +1 -f "$clog" | sed 's/^/[container] /' &
container_follow=$!

# A plain sleep 45 survives the kill below and holds this script's stdout open until it ends,
# so the wait is sliced and gives up the moment the game is gone.
( for ((i = 0; i < 45; i++)); do sleep 1; kill -0 "$game" 2>/dev/null || exit 0; done
  if [[ ! -f "$REPORT_DIR/Player.log" ]]; then
    echo "no Player.log after 45s; dumping state"
    docker ps -a --filter "name=$CONTAINER" --format '{{.Status}} {{.Image}}'
    /bin/ls -la "$REPORT_DIR" || true
  fi ) &
early=$!

( until [[ -f "$REPORT_DIR/Player.log" ]]; do sleep 1; done
  tail -n +1 -f "$REPORT_DIR/Player.log" | grep --line-buffered -oE 'pickle: .*' ) &
follow=$!

status=0
wait "$game" || status=$?
sleep 1
# The tails are grandchildren of a subshell or siblings of a pipeline, so killing the pids
# bash hands back leaves them running on this script's stdout forever.
kill "$follow" "$container_follow" "$early" 2>/dev/null || true
pkill -f "tail -n \+1 -f $clog" 2>/dev/null
pkill -f "tail -n \+1 -f $REPORT_DIR/Player.log" 2>/dev/null

summary="$REPORT_DIR/summary.json"
if [[ ! -f "$summary" || ! "$summary" -nt "$stamp" ]]; then
  echo "FAIL: this run wrote no summary.json, so it never reported (game exit $status)"
  echo "      read $REPORT_DIR/Player.log"
  if grep -qE 'XIO:.*fatal IO error|Desktop is 0 x 0' "$REPORT_DIR/Player.log" \
       "$clog" 2>/dev/null; then
    echo "      the X server died before the game got going - a flake, worth one retry"
    exit 75
  fi
  exit 1
fi

parsed="$(python3 -c 'import json, sys
d = json.load(open(sys.argv[1]))
print(d.get("total", 0))
print(d.get("failed", 1))
print(d.get("exitReason", "unknown"))' "$summary")" \
  || { echo "FAIL: $summary is not readable json"; exit 1; }
mapfile -t fields <<< "$parsed"
total="${fields[0]}" failed="${fields[1]}" reason="${fields[2]}"

if (( total == 0 )); then
  echo "FAIL: zero scenarios ran ($reason) - the filter '$SUITE_FILTER' matched nothing"
  echo "      a term wants the full feature file name or the mod name; a stem matches nothing"
  exit 1
fi

if (( failed != 0 )); then
  echo "FAIL: $failed of $total scenario(s) failed ($reason, game exit $status)"
  echo "      report: $REPORT_DIR"
  exit 1
fi

if [[ "$reason" != passed ]]; then
  echo "FAIL: no scenario failed but the run ended as '$reason' ($total ran, game exit $status)"
  echo "      report: $REPORT_DIR"
  exit 1
fi

if (( status != 0 )); then
  echo "note: every scenario passed but the game exited $status, which is xvfb-run teardown"
fi

echo "PASS: all $total scenario(s) passed - report: $REPORT_DIR"
