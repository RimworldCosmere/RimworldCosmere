using Cosmere.System.Scadrial.Utility;
using HarmonyLib;
using Verse;

namespace Cosmere.System.Scadrial.Patch;

[HarmonyPatch(typeof(DeepResourceGrid), nameof(DeepResourceGrid.AnyActiveDeepScannersOnMap))]
public static class PatchDeepResourceGrid {
    private static readonly AccessTools.FieldRef<DeepResourceGrid, Map> MapField =
        AccessTools.FieldRefAccess<DeepResourceGrid, Map>("map");

    [HarmonyPostfix]
    public static void Postfix(DeepResourceGrid __instance, ref bool __result) {
        if (__result) return;

        Map map = MapField(__instance);
        if (map == null) return;

        __result = AllomancyUtility.HasActiveBronzeSeeker(map);
    }
}