using Concord;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[Patch]
public abstract class RescueTrackingPatch : Pawn_RelationsTracker {
    [InjectField("pawn")]
    private readonly Pawn rescued = null!;

    protected RescueTrackingPatch(Pawn pawn) : base(pawn) { }

    [Inject(At.Return, nameof(Notify_RescuedBy))]
    private void AfterNotify_RescuedBy(Pawn rescuer) {
        Surgebinder? surgebinder = rescuer.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        rescuer.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_PawnsRescued, 1);

        if (rescued?.Faction == null) return;
        if (!rescued.Faction.HostileTo(rescuer.Faction)) return;

        rescuer.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_HostilePawnsRescued, 1);
    }
}
