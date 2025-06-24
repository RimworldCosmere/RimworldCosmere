using System.Linq;
using CosmereScadrial.Extensions;
using CosmereScadrial.Genes;
using CosmereScadrial.Things;
using Verse;
using Verse.AI;

namespace CosmereScadrial.JobGivers;

public class IngestVial : ThinkNode_JobGiver {
    public override float GetPriority(Pawn pawn) {
        return pawn.genes.GetMistingGenes().Any(x => x.ShouldConsumeVialNow()) ? 200f : 0f;
    }

    /**
     * @TODO Support multi-vials. Will have to re-enable multivials in the script generation when i get to that point
     */
    protected override Job? TryGiveJob(Pawn pawn) {
        if (pawn.Downed) return null;

        foreach (Allomancer gene in pawn.genes.GetMistingGenes()) {
            if (!gene.ShouldConsumeVialNow()) continue;
            AllomanticVial? vial = pawn.GetVials(gene).FirstOrDefault();
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