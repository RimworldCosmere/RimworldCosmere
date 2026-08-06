#!/usr/bin/env bash
# Build, verify, and launch RimWorld from THIS worktree.
#
# Exists because cwd does not persist between agent tool calls, so a bare
# `dotnet build Cosmere.slnx` silently builds the primary checkout and reports success.
# Everything here uses absolute paths derived from the script's own location.
#
#   ./scripts/worktree-run.sh [-a] [-m MARKER] [-p PROFILE] [-q QUICKSTART]
#     -a            also rebuild asset bundles (only when Assets/ changed)
#     -m MARKER     type name that must be present in the built DLL, verified before launch
#     -p PROFILE    docker-game profile (default: cosmere)
#     -q QUICKSTART quickstart name (default: PreCatacendreQuickstart)

set -euo pipefail

WT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DLL="$WT/CosmereCore/Assemblies/Cosmere.dll"
PROFILE=cosmere
QUICKSTART=PreCatacendreQuickstart
MARKER=""
ASSETS=0

while getopts "am:p:q:" opt; do
    case $opt in
        a) ASSETS=1 ;;
        m) MARKER="$OPTARG" ;;
        p) PROFILE="$OPTARG" ;;
        q) QUICKSTART="$OPTARG" ;;
        *) exit 2 ;;
    esac
done

echo "worktree: $WT"

echo "==> build"
dotnet build "$WT/Cosmere.slnx" | tail -3

if [[ -n "$MARKER" ]]; then
    echo "==> verify '$MARKER' is in the built DLL"
    found=$(strings "$DLL" | grep -c "$MARKER" || true)
    found_utf16=$(strings -el "$DLL" | grep -c "$MARKER" || true)
    if [[ "$found" -eq 0 && "$found_utf16" -eq 0 ]]; then
        echo "FAIL: '$MARKER' is not in $DLL. The build did not pick up your changes." >&2
        exit 1
    fi
    echo "    ok (utf8=$found utf16=$found_utf16)"
fi

if [[ "$ASSETS" -eq 1 ]]; then
    echo "==> asset bundles"
    (cd "$WT" && make build-assets | tail -2)
fi

echo "==> stop old container"
docker stop "docker-game-rimworld-$PROFILE" >/dev/null 2>&1 || true
rm -f "$HOME/.local/share/docker-game/rimworld/$PROFILE/.docker-game/lock"

echo "==> launch"
(cd "$WT" && docker-game rimworld "$PROFILE" -- "-cosmerequickstart=$QUICKSTART") >/dev/null 2>&1 &

until docker ps --filter "name=docker-game-rimworld-$PROFILE" --format '{{.Names}}' | grep -q "$PROFILE"; do
    sleep 1
done

echo "==> verify container bound this worktree"
bound=$(docker inspect "docker-game-rimworld-$PROFILE" \
    --format '{{range .Mounts}}{{.Source}}{{"\n"}}{{end}}' | grep -c "$(basename "$WT")" || true)
if [[ "$bound" -ne 3 ]]; then
    echo "FAIL: expected 3 mods bound from $(basename "$WT"), got $bound." >&2
    exit 1
fi
echo "    ok (3/3)"

if [[ -n "$MARKER" ]]; then
    echo "==> verify container DLL has '$MARKER'"
    in_container=$(docker exec "docker-game-rimworld-$PROFILE" sh -c \
        "{ strings /game/Mods/Cosmere.Core/Assemblies/Cosmere.dll; strings -el /game/Mods/Cosmere.Core/Assemblies/Cosmere.dll; } | grep -c '$MARKER'" || true)
    if [[ "$in_container" -eq 0 ]]; then
        echo "FAIL: container is running a stale DLL." >&2
        exit 1
    fi
    echo "    ok"
fi

echo "==> waiting for load"
RUNS="$HOME/.local/share/docker-game/rimworld/$PROFILE/logs/runs"
D=$(ls -1t "$RUNS" | head -1)
LOG="$RUNS/$D/Player.log"
for _ in $(seq 1 90); do
    grep -q "Game loaded and ready" "$LOG" 2>/dev/null && break
    sleep 2
done

echo "log: $LOG"
grep -c "Game loaded and ready" "$LOG" | sed 's/^/    loaded: /'
