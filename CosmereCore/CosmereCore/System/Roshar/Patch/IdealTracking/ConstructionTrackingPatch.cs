using Concord;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[Patch]
public abstract class ConstructionTrackingPatch : Frame {
    [Inject(At.Return, nameof(CompleteConstruction))]
    private void AfterCompleteConstruction(Pawn worker) {
        if (worker == null) return;

        Surgebinder? surgebinder = worker.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        worker.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_StructuresBuilt, 1);
    }
}
