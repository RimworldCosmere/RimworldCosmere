using HarmonyLib;
using RimWorld;
using Verse;
using Cosmere.System.Scadrial.Gene;
using Cosmere.System.Scadrial.Hemalurgy;
using Cosmere.System.Scadrial.Hemalurgy.Hediff;
using Cosmere.System.Scadrial.Util;

namespace Cosmere.System.Scadrial.Patch.World;

[HarmonyPatch(typeof(StorytellerUtility), nameof(StorytellerUtility.DefaultThreatPointsNow))]
[HarmonyPatch([typeof(IIncidentTarget)])]
public static class ScadrialStorytellerUtilityPatch {
    private const float MistbornOrFullFeruchemistBonus = 150f;
    private const float SingleMetalBonus = 20f;
    private const float ThreatPerSpike = 30f;

    [HarmonyPostfix]
    public static void Postfix(ref float __result, IIncidentTarget target) {
        Map? map = target as Map;
        if (map == null) return;

        float bonus = 0f;
        List<Pawn> colonists = map.mapPawns.FreeColonists;
        for (int i = 0; i < colonists.Count; i++) {
            bonus += GetThreatBonusForPawn(colonists[i]);
        }

        __result += bonus;

        float copperReduction = AllomancyUtility.GetCoppercloudStrength(map);
        if (copperReduction > 0f) {
            __result *= 1f - copperReduction;
        }
    }

    private static float GetThreatBonusForPawn(Pawn pawn) {
        if (pawn.genes == null || pawn.story?.traits == null) return 0f;

        float bonus = 0f;
        if (pawn.IsMistborn() || pawn.IsFullFeruchemist()) {
            bonus += MistbornOrFullFeruchemistBonus;
        }
        else {
            List<Allomancer> allomancerGenes = pawn.genes.GetAllomanticGenes();
            for (int i = 0; i < allomancerGenes.Count; i++) {
                bonus += SingleMetalBonus;
            }

            List<Feruchemist> feruchemistGenes = pawn.genes.GetFeruchemicGenes();
            for (int i = 0; i < feruchemistGenes.Count; i++) {
                bonus += SingleMetalBonus;
            }
        }

        HemalurgicSpikes? spikesHediff = (HemalurgicSpikes?)pawn.health.hediffSet.GetFirstHediffOfDef(
            HemalurgicDefOf.Cosmere_Scadrial_Hediff_HemalurgicSpikes
        );
        if (spikesHediff != null) {
            bonus += spikesHediff.spikeCount * ThreatPerSpike;
        }

        return bonus;
    }
}