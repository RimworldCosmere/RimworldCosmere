using HarmonyLib;
using RimWorld;
using UnityEngine;

namespace Cosmere.Core.Patch;

[HarmonyPatch(typeof(Dialog_ModSettings), nameof(Dialog_ModSettings.InitialSize), MethodType.Getter)]
public static class CosmereSettingsWindowSizePatch {
    public static void Postfix(Dialog_ModSettings __instance, ref Vector2 __result) {
        Verse.Mod? mod = AccessTools.Field(typeof(Dialog_ModSettings), "mod").GetValue(__instance) as Verse.Mod;
        if (mod is not Mod) return;

        float targetWidth = Mathf.Min(1200f, Verse.UI.screenWidth - 40f);
        float targetHeight = Mathf.Min(1000f, Verse.UI.screenHeight - 40f);
        __result = new Vector2(Mathf.Max(__result.x, targetWidth), Mathf.Max(__result.y, targetHeight));
    }
}