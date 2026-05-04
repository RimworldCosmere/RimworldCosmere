using HarmonyLib;
using Verse;
using Cosmere.System.Roshar.Util;

namespace Cosmere.System.Roshar.Patch.Highstorm;

[HarmonyPatch(typeof(Region), nameof(Region.DangerFor))]
public static class HighstormDangerPatch {
    [HarmonyPostfix]
    public static void Postfix(Region __instance, Pawn p, ref Danger __result) {
        if (__result == Danger.Deadly) return;

        Room room = __instance.Room;
        if (room == null || !room.PsychologicallyOutdoors) return;

        Map map = __instance.Map;
        if (map == null) return;

        if (StormlightUtility.IsHighstormImmune(p)) return;

        List<RimWorld.GameCondition> conditions = map.gameConditionManager.ActiveConditions;
        for (int i = 0; i < conditions.Count; i++) {
            if (conditions[i] is Cosmere.System.Roshar.GameCondition.Highstorm hs && hs.IsDangerousPhase) {
                __result = Danger.Deadly;
                return;
            }
        }
    }
}