using Cosmere.Lightweave.ModsConfig;
using Cosmere.Lightweave.Settings;
using HarmonyLib;
using Verse;

namespace Cosmere.Lightweave.Patch;

[HarmonyPatch(typeof(Verse.ModsConfig), nameof(Verse.ModsConfig.RestartFromChangedMods))]
public static class ModsConfigRestartPatch {
    public static bool Prefix() {
        LightweaveSettings? settings = LightweaveMod.Settings;
        if (settings == null || !settings.RedesignMainMenu) {
            return true;
        }
        Find.WindowStack.Add(new Dialog_ModsConfigRestart());
        return false;
    }
}
