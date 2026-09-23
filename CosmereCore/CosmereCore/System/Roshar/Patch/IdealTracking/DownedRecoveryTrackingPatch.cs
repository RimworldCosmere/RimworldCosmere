using Concord;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[Patch]
public abstract class DownedRecoveryTrackingPatch : Pawn_HealthTracker {
    [InjectField("pawn")]
    private readonly Pawn trackedPawn = null!;

    protected DownedRecoveryTrackingPatch(Pawn pawn) : base(pawn) { }

    [Inject(At.Return, "MakeUndowned")]
    private void AfterMakeUndowned() {
        if (trackedPawn == null) return;

        Surgebinder? surgebinder = trackedPawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        trackedPawn.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_ChallengesSurvived, 1);
    }
}
