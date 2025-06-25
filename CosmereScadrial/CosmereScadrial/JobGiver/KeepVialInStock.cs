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
            ThingDef vialDef = gene.metal.GetVial();
            int inStock = pawn.inventory.innerContainer.TotalStackCountOfDef(vialDef);
            if (inStock >= gene.RequestedVialStock) continue;

            int amountToTake = gene.RequestedVialStock - inStock;

            Verse.Thing? thing = FindVialFor(pawn, vialDef, amountToTake);
            if (thing == null) continue;

            Job job = JobMaker.MakeJob(RimWorld.JobDefOf.TakeInventory, thing);
            job.count = Math.Min(amountToTake, thing.def.orderedTakeGroup.max - inStock);

            return job;
        }

        return null;
    }

    private Verse.Thing? FindVialFor(Pawn pawn, ThingDef vialDef, int desired) {
        return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(vialDef),
            PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, x => VialValidator(pawn, x, desired));
    }

    private bool VialValidator(Pawn pawn, Verse.Thing vial, int desired) {
        if (!vial.Spawned) return true;
        if (vial.IsForbidden(pawn)) return false;

        return pawn.CanReserveAndReach(vial, PathEndMode.ClosestTouch, Danger.None, 10,
            Math.Min(vial.def.orderedTakeGroup.max, desired));
    }
}