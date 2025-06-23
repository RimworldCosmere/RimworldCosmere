using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Genes;
using CosmereScadrial.Things;
using Verse;
using Verse.AI;

namespace CosmereScadrial.JobGivers;

public class IngestVial : ThinkNode_JobGiver {
    private List<Allomancer> GetGenes(Pawn pawn) {
        return pawn.genes?.GenesListForReading.Where(x => x is Allomancer).Cast<Allomancer>().ToList() ?? [];
    }

    public override float GetPriority(Pawn pawn) {
        return GetGenes(pawn).Any(x => x.ShouldConsumeVialNow()) ? 200f : 0f;
    }

    /**
     * @TODO Support multi-vials. Will have to re-enable multivials in the script generation when i get to that point
     */
    protected override Job? TryGiveJob(Pawn pawn) {
        if (pawn.Downed) return null;

        List<Allomancer> genes = GetGenes(pawn);
        foreach (Allomancer? gene in GetGenes(pawn)) {
            if (!gene.ShouldConsumeVialNow()) continue;
            AllomanticVial? vial = GetVial(pawn, gene);
            if (vial == null) continue;
            if (!pawn.CanReserveAndReach(vial, PathEndMode.InteractionCell, Danger.Some,
                    ignoreOtherReservations: true)) {
                return null;
            }

            Job job = JobMaker.MakeJob(JobDefOf.Cosmere_Job_IngestVial, vial);
            job.count = 1;
            job.ingestTotalCount = true;

            return job;
        }

        return null;
    }

    private AllomanticVial? GetVial(Pawn pawn, Allomancer gene) {
        return Enumerable.FirstOrDefault(GetVials(pawn), x => x.IsForMetal(gene.metal));
    }

    private List<AllomanticVial> GetVials(Pawn pawn) {
        return pawn.inventory?.innerContainer?
            .Where(x => x is AllomanticVial && x is not AllomanticMetal).Cast<AllomanticVial>().ToList() ?? [];
    }
}