using Concord;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.World;

[Patch(typeof(StorytellerUtility))]
public static class RosharStorytellerUtilityPatch {
    private const float ThreatPerIdeal = 40f;

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
            Pawn pawn = colonists[i];
            if (pawn.genes == null) continue;

            List<Verse.Gene> genes = pawn.genes.GenesListForReading;
            for (int j = 0; j < genes.Count; j++) {
                if (genes[j] is Surgebinder { Active: true } surgebinder) {
                    bonus += surgebinder.CurrentIdealDisplay * ThreatPerIdeal;
                }
            }
        }

        ch.ReturnValue += bonus;
    }
}
