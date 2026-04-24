using Cosmere.System.Roshar.GameCondition;
using Cosmere.System.Roshar.Utility;
using HarmonyLib;
using Verse;

namespace Cosmere.System.Roshar.Patch;

[HarmonyPatch(typeof(Region), nameof(Region.DangerFor))]
public static class HighstormDangerPatch {
    [HarmonyPostfix]
    public static void Postfix(Region __instance, Pawn p, ref Danger __result) {
        if (__result == Danger.Deadly) return;

        Room room = __instance.Room;
        if (room == null || !room.PsychologicallyOutdoors) return;

        Map map = __instance.Map;
        if (map == null) return;

        if (StormlightUtilities.IsHighstormImmune(p)) return;

        List<RimWorld.GameCondition> conditions = map.gameConditionManager.ActiveConditions;
        for (int i = 0; i < conditions.Count; i++) {
            if (conditions[i] is Highstorm hs && hs.IsDangerousPhase) {
                __result = Danger.Deadly;
                return;
            }
        }
    }
}