using Concord;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[Patch]
public abstract class ArrestTrackingPatch : Pawn_GuestTracker {
    protected ArrestTrackingPatch(Pawn pawn) : base(pawn) { }

    [Inject(At.Return, nameof(CapturedBy))]
    private void AfterCapturedBy(Faction by, Pawn byPawn) {
        if (byPawn == null) return;
        if (by != Faction.OfPlayer) return;

        Surgebinder? surgebinder = byPawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        byPawn.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_ArrestsMade, 1);
    }
}
