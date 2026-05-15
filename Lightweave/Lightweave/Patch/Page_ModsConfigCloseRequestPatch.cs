using Cosmere.Lightweave.ModsConfig;
using Cosmere.Lightweave.Settings;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.Lightweave.Patch;

[HarmonyPatch(typeof(Page_ModsConfig), nameof(Page_ModsConfig.OnCloseRequest))]
public static class Page_ModsConfigCloseRequestPatch {
    public static bool Prefix(Page_ModsConfig __instance, ref bool __result) {
        LightweaveSettings? settings = LightweaveMod.Settings;
        if (settings == null || !settings.RedesignMainMenu) {
            return true;
        }

        if (ModsConfigState.GetSaveChanges(__instance) || ModsConfigState.GetDiscardChanges(__instance)) {
            __result = true;
            return false;
        }

        if (!ModsConfigState.HasUnsavedChanges(__instance)) {
            ModsConfigState.SetDiscardChanges(__instance, true);
            __result = true;
            return false;
        }

        Find.WindowStack.Add(new Dialog_ModsConfigConfirmClose(
            onSave: () => {
                ModsConfigState.SetSaveChanges(__instance, true);
                __instance.Close();
            },
            onDiscard: () => {
                ModsConfigState.SetDiscardChanges(__instance, true);
                __instance.Close();
            }
        ));
        __result = false;
        return false;
    }
}
