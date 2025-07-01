using System;
using CosmereFramework.Extension;
using CosmereScadrial.Extension;
using CosmereScadrial.Gene;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereScadrial.JobGiver;

public class KeepVialInStock : ThinkNode_JobGiver {
    protected override Job? TryGiveJob(Pawn pawn) {
        if (pawn.Downed || pawn.IsAsleep()) return null;

        foreach (Allomancer gene in pawn.genes.GetAllomanticGenes()) {
            int inStock =
                pawn.inventory.innerContainer.TotalStackCountOfDef(ThingDefOf.Cosmere_Scadrial_Thing_AllomanticVial,
                    gene.metal.Item);
            if (inStock >= gene.RequestedVialStock) continue;

            int amountToTake = gene.RequestedVialStock - inStock;

            Verse.Thing? thing = FindVialFor(pawn, gene, amountToTake);
            if (thing == null) continue;

            Job job = JobMaker.MakeJob(RimWorld.JobDefOf.TakeInventory, thing);
            job.count = Math.Min(amountToTake, thing.def.orderedTakeGroup.max - inStock);

            return job;
        }

        return null;
    }

    private Verse.Thing? FindVialFor(Pawn pawn, Allomancer gene, int desired) {
        return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
            ThingRequest.ForDef(ThingDefOf.Cosmere_Scadrial_Thing_AllomanticVial),
            PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, x => VialValidator(pawn, gene, x, desired));
    }

    private bool VialValidator(Pawn pawn, Allomancer gene, Verse.Thing vial, int desired) {
        if (!vial.Spawned) return true;
        if (vial.IsForbidden(pawn)) return false;
        if (!vial.Stuff.Equals(gene.metal.Item)) return false;

        return pawn.CanReserveAndReach(vial, PathEndMode.ClosestTouch, Danger.None, 10,
            Math.Min(vial.def.orderedTakeGroup.max, desired));
    }
}