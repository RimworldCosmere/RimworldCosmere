using Concord;
using RimWorld;
using Verse;
using GeneUtility = Cosmere.System.Scadrial.Util.GeneUtility;

namespace Cosmere.System.Scadrial.Patch.Gene;

[Patch]
public abstract class PawnRelationWorkerChildPatch : PawnRelationWorker_Child {
    [Inject(At.Return, nameof(CreateRelation))]
    private void AfterCreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request) {
        GeneUtility.AssignScadrialGenes(generated);
        GeneUtility.EnforceSkaaPurityInheritance(generated, other, request);
    }
}
