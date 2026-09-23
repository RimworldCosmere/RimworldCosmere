#!/usr/bin/env bash
# stage-pickle-mods.sh <mods-dir> <config-dir>
#
# Downloads the mods a Pickle run needs and writes the config the game reads at boot.
# gamecrate does this locally; CI has no gamecrate. Run it to reproduce what CI stages.
set -uo pipefail

MODS="${1:?usage: stage-pickle-mods.sh <mods-dir> <config-dir>}"
CONFIG="${2:?usage: stage-pickle-mods.sh <mods-dir> <config-dir>}"
WORK="${RUNNER_TEMP:-${TMPDIR:-/tmp}}"

mkdir -p "$MODS" "$CONFIG"
command -v gh >/dev/null || { echo "gh is not on PATH" >&2; exit 1; }
command -v unzip >/dev/null || { echo "unzip is not on PATH" >&2; exit 1; }

# Core needs Concord and RimLogging, Pickle runs the suite, and every feature opens
# with a @quickstart: tag. Versions float, same as the csproj pins on these three.
for spec in ConcordLib/RimWorld:Concord \
            RimWorks/rimworld-logging-framework:RimLogging \
            RimWorks/Rimworld-Quickstarts:Quickstarts \
            RimWorks/Rimworld-Pickle:Pickle; do
  name="${spec##*:}"
  if ! gh release download --repo "${spec%%:*}" --pattern "$name-*.zip" --dir "$WORK" --clobber; then
    echo "could not download a release for ${spec%%:*}" >&2
    exit 1
  fi
  unzip -qo "$WORK/$name-"*.zip -d "$MODS" || exit 1
  echo "staged $name"
done

# Concord loads before the game, then the tooling, then Cosmere.
cat > "$CONFIG/ModsConfig.xml" <<'MODSEOF'
<?xml version="1.0" encoding="utf-8"?>
<ModsConfigData>
  <version>1.6</version>
  <activeMods>
    <li>concordlib.concord</li>
    <li>ludeon.rimworld</li>
    <li>ludeon.rimworld.royalty</li>
    <li>ludeon.rimworld.ideology</li>
    <li>ludeon.rimworld.biotech</li>
    <li>ludeon.rimworld.anomaly</li>
    <li>ludeon.rimworld.odyssey</li>
    <li>rimworks.rimlogging</li>
    <li>rimworks.quickstarts</li>
    <li>rimworks.pickle</li>
    <li>cosmere.core</li>
    <li>cosmere.scadrial</li>
    <li>cosmere.roshar</li>
  </activeMods>
  <knownExpansions>
    <li>ludeon.rimworld.royalty</li>
    <li>ludeon.rimworld.ideology</li>
    <li>ludeon.rimworld.biotech</li>
    <li>ludeon.rimworld.anomaly</li>
    <li>ludeon.rimworld.odyssey</li>
  </knownExpansions>
</ModsConfigData>
MODSEOF

cat > "$CONFIG/Prefs.xml" <<'PREFSEOF'
<?xml version="1.0" encoding="utf-8"?>
<PrefsData>
  <screenWidth>1920</screenWidth>
  <screenHeight>1080</screenHeight>
  <fullscreen>False</fullscreen>
  <volumeGame>0</volumeGame>
  <volumeMusic>0</volumeMusic>
  <volumeAmbient>0</volumeAmbient>
  <devMode>True</devMode>
  <runInBackground>True</runInBackground>
  <resetModsConfigOnCrash>False</resetModsConfigOnCrash>
</PrefsData>
PREFSEOF

# The game writes here as another uid inside the container.
chmod -R 777 "$CONFIG"

echo "staged mods into $MODS, config into $CONFIG"
