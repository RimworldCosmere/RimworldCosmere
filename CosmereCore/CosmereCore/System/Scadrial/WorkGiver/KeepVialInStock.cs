using System;
using System.Collections.Generic;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.WorkGiver;

public class KeepVialInStock : WorkGiver_Scanner {
    private static ThingDef vialDef => ThingDefOf.Cosmere_Scadrial_Thing_AllomanticVial;

    public override PathEndMode PathEndMode => Verse.AI.PathEndMode.ClosestTouch;

    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(vialDef);

    public override IEnumerable<Verse.Thing> PotentialWorkThingsGlobal(Pawn pawn) {
        return pawn.Map.listerThings.ThingsOfDef(vialDef);
    }

    public override bool HasJobOnThing(Pawn pawn, Verse.Thing t, bool forced = false) {
        if (pawn.Downed || pawn.IsAsleep()) return false;
        if (!t.Spawned || t.IsForbidden(pawn)) return false;
        if (GeneWanting(pawn, t) == null) return false;

        return pawn.CanReserveAndReach(t, PathEndMode.ClosestTouch, Danger.None, 10, 1);
    }

    public override Job? JobOnThing(Pawn pawn, Verse.Thing t, bool forced = false) {
        Allomancer? gene = GeneWanting(pawn, t);
        if (gene == null) return null;

        int inStock = StockOf(pawn, gene);
        Job job = JobMaker.MakeJob(RimWorld.JobDefOf.TakeInventory, t);
        job.count = Math.Min(gene.RequestedVialStock - inStock, t.def.orderedTakeGroup.max - inStock);

        return job;
    }

    // Keyed off the vial's own metal rather than the first gene short of stock, or a pawn short
    // of two metals only ever restocks whichever gene the gene list happens to yield first.
    private static Allomancer? GeneWanting(Pawn pawn, Verse.Thing vial) {
        if (pawn.genes == null) return null;

        foreach (Allomancer gene in pawn.genes.GetAllomanticGenes()) {
            if (vial.Stuff != gene.metal.Item) continue;
            if (StockOf(pawn, gene) >= gene.RequestedVialStock) continue;

            return gene;
        }

        return null;
    }

    private static int StockOf(Pawn pawn, Allomancer gene) {
        return pawn.inventory.innerContainer.TotalStackCountOfDef(vialDef, gene.metal.Item);
    }
}
