using System;
using System.Collections.Generic;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.WorkGiver;

public class KeepVialInStock : WorkGiver_Scanner {
    private ThingDef vialDef => ThingDefOf.Cosmere_Scadrial_Thing_AllomanticVial;

    public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(vialDef);

    public override IEnumerable<Verse.Thing> PotentialWorkThingsGlobal(Pawn pawn) {
        return pawn.Map.listerThings.ThingsOfDef(vialDef);
    }

    public override bool HasJobOnThing(Pawn pawn, Verse.Thing t, bool forced = false) {
        if (pawn.Downed || pawn.IsAsleep()) return false;
        if (pawn.genes == null) return false;
        if (t is not Verse.Thing vial) return false;

        // Check if this pawn needs any vials
        bool needsVial = false;
        Allomancer? neededGene = null;
        
        foreach (Allomancer gene in pawn.genes.GetAllomanticGenes()) {
            int inStock = pawn.inventory.innerContainer.TotalStackCountOfDef(
                vialDef,
                gene.metal.Item
            );
            if (inStock < gene.RequestedVialStock) {
                needsVial = true;
                neededGene = gene;
                break;
            }
        }

        if (!needsVial || neededGene == null) return false;

        // Validate the vial matches what we need
        if (!vial.Spawned) return false;
        if (vial.IsForbidden(pawn)) return false;
        if (!vial.Stuff.Equals(neededGene.metal.Item)) return false;

        return pawn.CanReserveAndReach(vial, PathEndMode.ClosestTouch, Danger.None, 10, 1);
    }

    public override Job? JobOnThing(Pawn pawn, Verse.Thing t, bool forced = false) {
        if (t is not Verse.Thing vial) return null;

        // Find which gene needs vials
        foreach (Allomancer gene in pawn.genes.GetAllomanticGenes()) {
            int inStock = pawn.inventory.innerContainer.TotalStackCountOfDef(
                vialDef,
                gene.metal.Item
            );
            if (inStock >= gene.RequestedVialStock) continue;
            if (!vial.Stuff.Equals(gene.metal.Item)) continue;

            int amountToTake = gene.RequestedVialStock - inStock;
            
            Job job = JobMaker.MakeJob(RimWorld.JobDefOf.TakeInventory, vial);
            job.count = Math.Min(amountToTake, vial.def.orderedTakeGroup.max - inStock);

            return job;
        }

        return null;
    }
}
