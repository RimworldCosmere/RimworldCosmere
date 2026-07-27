using Cosmere.System.Roshar.Gene;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.World;

[HarmonyPatch(typeof(StorytellerUtility), nameof(StorytellerUtility.DefaultThreatPointsNow))]
[HarmonyPatch([typeof(IIncidentTarget)])]
public static class RosharStorytellerUtilityPatch {
    private const float ThreatPerIdeal = 40f;

    [HarmonyPostfix]
    public static void Postfix(ref float __result, IIncidentTarget target) {
        Map? map = target as Map;
        if (map == null) return;

        float bonus = 0f;
        List<Pawn> colonists = map.mapPawns.FreeColonists;
        for (int i = 0; i < colonists.Count; i++) {
            Pawn pawn = colonists[i];
            if (pawn.genes == null) continue;

            List<Verse.Gene> genes = pawn.genes.GenesListForReading;
            for (int j = 0; j < genes.Count; j++) {
                if (genes[j] is Surgebinder { Active: true } surgebinder) {
                    bonus += surgebinder.CurrentIdealDisplay * ThreatPerIdeal;
                }
            }
        }

        __result += bonus;
    }
}
