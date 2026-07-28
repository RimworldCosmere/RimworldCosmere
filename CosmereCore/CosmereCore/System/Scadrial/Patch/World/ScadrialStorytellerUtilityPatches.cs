using Concord;
using Cosmere.System.Scadrial.Gene;
using Cosmere.System.Scadrial.Hemalurgy;
using Cosmere.System.Scadrial.Hemalurgy.Hediff;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Patch.World;

[Patch(typeof(StorytellerUtility))]
public static class ScadrialStorytellerUtilityPatch {
    private const float MistbornOrFullFeruchemistBonus = 150f;
    private const float SingleMetalBonus = 20f;
    private const float ThreatPerSpike = 30f;

    [Inject(
        At.Return,
        nameof(StorytellerUtility.DefaultThreatPointsNow),
        parameterTypes: [typeof(IIncidentTarget)]
    )]
    private static void AfterDefaultThreatPointsNow(IIncidentTarget target, ControlHandle<float> ch) {
        Map? map = target as Map;
        if (map == null) return;

        float bonus = 0f;
        List<Pawn> colonists = map.mapPawns.FreeColonists;
        for (int i = 0; i < colonists.Count; i++) {
            bonus += GetThreatBonusForPawn(colonists[i]);
        }

        ch.ReturnValue += bonus;

        float copperReduction = AllomancyUtility.GetCoppercloudStrength(map);
        if (copperReduction > 0f) {
            ch.ReturnValue *= 1f - copperReduction;
        }
    }

    private static float GetThreatBonusForPawn(Pawn pawn) {
        if (pawn.genes == null || pawn.story?.traits == null) return 0f;

        float bonus = 0f;
        if (pawn.IsMistborn() || pawn.IsFullFeruchemist()) {
            bonus += MistbornOrFullFeruchemistBonus;
        } else {
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
