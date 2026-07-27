using HarmonyLib;
using Verse;

namespace Cosmere.Core.Lib.FloatSubMenu;

[HarmonyPatch]
internal static class Patches {
    private static bool replaceDist;
    private static float dist;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(FloatMenu), "UpdateBaseColor")]
    public static void UpdateBaseColor_Pre(FloatMenu __instance) {
        // Make sure we do not replace any values needed to calculate replacement.
        replaceDist = false;
        replaceDist = FloatSubMenu.ShouldReplaceDistanceFor(__instance, ref dist);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(FloatMenu), "UpdateBaseColor")]
    public static void UpdateBaseColor_Post() {
        replaceDist = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GenUI), nameof(GenUI.DistFromRect))]
    public static bool DistFromRect_Pre(ref float __result) {
        if (replaceDist) {
            __result = dist;
            return false;
        }

        return true;
    }
}
