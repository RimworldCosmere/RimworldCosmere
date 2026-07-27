using Cosmere.System.Scadrial.Gene;
using Cosmere.System.Scadrial.Thing;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.JobGiver;

public class IngestVial : ThinkNode_JobGiver {
    public override float GetPriority(Pawn pawn) {
        if (pawn.genes == null) return 0f;
        return pawn.genes.GetAllomanticGenes().Any(x => x.ShouldConsumeVialNow) ? 200f : 0f;
    }

    protected override Job? TryGiveJob(Pawn pawn) {
        if (pawn.Downed) return null;
        if (pawn.genes == null) return null;

        foreach (Allomancer gene in pawn.genes.GetAllomanticGenes()) {
            if (!gene.ShouldConsumeVialNow) continue;
            AllomanticVial? vial = pawn.GetVial(gene);
            if (vial == null) continue;
            if (!pawn.CanReserveAndReach(vial, PathEndMode.InteractionCell, Danger.Some)) {
                return null;
            }

            Job job = JobMaker.MakeJob(RimWorld.JobDefOf.Ingest, vial);
            job.count = 1;
            job.ingestTotalCount = true;

            return job;
        }

        return null;
    }
}
