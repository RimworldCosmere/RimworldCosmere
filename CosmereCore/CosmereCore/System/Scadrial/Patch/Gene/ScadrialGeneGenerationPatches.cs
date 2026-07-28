using Concord;
using Verse;
using GeneUtility = Cosmere.System.Scadrial.Util.GeneUtility;

namespace Cosmere.System.Scadrial.Patch.Gene;

[Patch(typeof(Verse.PawnGenerator))]
public static class ScadrialGeneGenerationPatch {
    [Inject(At.Return, "GenerateGenes")]
    private static void AfterGenerateGenes(Pawn? pawn, PawnGenerationRequest request) {
        if (pawn?.RaceProps.Humanlike != true) {
            return;
        }

        GeneUtility.AssignScadrialGenes(pawn);
    }
}
