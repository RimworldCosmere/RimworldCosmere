using System.Collections.Generic;
using System.Linq;
using CosmereResources.Defs;
using CosmereScadrial.Genes;
using CosmereScadrial.Things;
using Verse;

namespace CosmereScadrial.Extensions;

public static class Pawn_InventoryTrackerExtension {
    public static IEnumerable<AllomanticVial> GetVials(this Pawn pawn) {
        return pawn.inventory?.innerContainer?
            .Where(x => x is AllomanticVial && x is not AllomanticMetal).Cast<AllomanticVial>().ToList() ?? [];
    }

    public static IEnumerable<AllomanticVial> GetVials(this Pawn pawn, Allomancer gene) {
        return pawn.GetVials().Where(x => x.IsForMetal(gene.metal));
    }

    public static IEnumerable<AllomanticVial> GetVials(this Pawn pawn, MetalDef metal) {
        Allomancer? gene = pawn.genes.GetAllomanticGeneForMetal(metal);

        return gene == null ? [] : pawn.GetVials(gene);
    }

    public static bool HasVial(this Pawn pawn, MetalDef metal) {
        return pawn.GetVials(metal).Any();
    }
}