using HarmonyLib;
using RimWorld;
using Verse;
using Cosmere.Lightweave.Settings;

namespace Cosmere.Lightweave.Patch;

[HarmonyPatch(typeof(Window), "get_Margin")]
public static class Window_MarginPatch {
    public static void Postfix(Window __instance, ref float __result) {
        LightweaveSettings? settings = LightweaveMod.Settings;
        if (settings is not { RedesignMainMenu: true }) return;
        if (__instance is Dialog_Options
            or Page_ModsConfig
            or Dialog_SaveFileList_Load) {
            __result = 0f;
        }
    }
}
