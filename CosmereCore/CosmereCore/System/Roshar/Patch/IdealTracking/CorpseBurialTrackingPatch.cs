using Concord;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[Patch]
public abstract class CorpseBurialTrackingPatch : Building_Grave {
    [Inject(At.Return, nameof(TryAcceptThing))]
    private void AfterTryAcceptThing(Verse.Thing thing, ControlHandle<bool> ch) {
        if (!ch.ReturnValue) return;
        if (thing is not Verse.Corpse) return;
        if (Map == null) return;

        Building_Grave self = this;
        List<Pawn> colonists = Map.mapPawns.FreeColonistsSpawned;
        for (int i = 0; i < colonists.Count; i++) {
            Pawn colonist = colonists[i];
            Verse.AI.Job job = colonist.CurJob;
            if (job == null) continue;
            if (job.def != RimWorld.JobDefOf.HaulToContainer) continue;
            if (job.targetB.Thing != self) continue;

            Surgebinder? surgebinder = colonist.genes?.GetFirstGeneOfType<Surgebinder>();
            if (surgebinder == null) continue;

            colonist.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_DeadHonored, 1);
            break;
        }
    }
}
