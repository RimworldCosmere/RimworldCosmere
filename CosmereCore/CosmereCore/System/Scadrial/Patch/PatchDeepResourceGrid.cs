using HarmonyLib;
using Verse;

namespace Cosmere.System.Scadrial.Patch;

[HarmonyPatch(typeof(DeepResourceGrid), nameof(DeepResourceGrid.AnyActiveDeepScannersOnMap))]
public static class PatchDeepResourceGrid {
    [HarmonyPostfix]
    public static void Postfix(DeepResourceGrid __instance, ref bool __result) {
        if (__result) return;

        Map map = Traverse.Create(__instance).Field("map").GetValue<Map>();
        if (map == null) return;

        __result = Utility.AllomancyUtility.HasActiveBronzeSeeker(map);
    }
}
