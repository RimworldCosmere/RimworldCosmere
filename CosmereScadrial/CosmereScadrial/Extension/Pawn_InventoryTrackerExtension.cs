using System.Collections.Generic;
using System.Linq;
using CosmereResources.Def;
using CosmereScadrial.Gene;
using CosmereScadrial.Thing;
using Verse;

namespace CosmereScadrial.Extension;

public static class Pawn_InventoryTrackerExtension {
    public static IEnumerable<AllomanticVial> GetVials(this Pawn pawn) {
        return pawn.inventory?.innerContainer?
            .Where(x => x is AllomanticVial and not AllomanticMetal).Cast<AllomanticVial>().ToList() ?? [];
    }

    public static AllomanticVial? GetVial(this Pawn pawn, Allomancer gene) {
        return pawn.GetVials().FirstOrDefault(x => x.IsForMetal(gene.metal));
    }

    public static AllomanticVial? GetVial(this Pawn pawn, MetalDef metal) {
        Allomancer? gene = pawn.genes.GetAllomanticGeneForMetal(metal);

        return gene == null ? null : pawn.GetVial(gene);
    }

    public static bool HasVial(this Pawn pawn, MetalDef metal) {
        return pawn.GetVial(metal) != null;
    }
}