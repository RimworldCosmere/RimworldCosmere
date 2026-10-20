using Concord;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[Patch]
public abstract class RelationshipLossTrackingPatch : Pawn_RelationsTracker {
    [InjectField("pawn")]
    private readonly Pawn trackedPawn = null!;

    protected RelationshipLossTrackingPatch(Pawn pawn) : base(pawn) { }

    [Inject(
        At.Head,
        nameof(RemoveDirectRelation),
        parameterTypes: [typeof(PawnRelationDef), typeof(Pawn)]
    )]
    private void BeforeRemoveDirectRelation(PawnRelationDef def, Pawn otherPawn) {
        if (trackedPawn == null || trackedPawn.Dead) return;

        if (!IsCloseRelation(def)) return;

        Surgebinder? surgebinder = trackedPawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        surgebinder.OnRelationshipLost();
    }

    private static bool IsCloseRelation(PawnRelationDef def) {
        return def == PawnRelationDefOf.Spouse ||
               def == PawnRelationDefOf.Fiance ||
               def == PawnRelationDefOf.Lover ||
               def == PawnRelationDefOf.Parent ||
               def == PawnRelationDefOf.Child ||
               def == PawnRelationDefOf.Bond;
    }
}
