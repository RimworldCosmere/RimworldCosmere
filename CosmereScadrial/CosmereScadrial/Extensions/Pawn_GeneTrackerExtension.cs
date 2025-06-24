using System.Collections.Generic;
using System.Linq;
using CosmereResources.Defs;
using CosmereScadrial.Genes;
using RimWorld;
using Verse;

namespace CosmereScadrial.Extensions;

public static class Pawn_GeneTrackerExtension {
    public static Allomancer? GetAllomanticGeneForMetal(this Pawn_GeneTracker genes, MetalDef metal) {
        GeneDef geneDef = GeneDefOf.GetMistingGeneForMetal(metal);

        return (Allomancer)genes.GetGene(geneDef);
    }

    public static bool HasAllomanticGeneForMetal(this Pawn_GeneTracker genes, MetalDef metal) {
        return genes.HasActiveGene(GeneDefOf.GetMistingGeneForMetal(metal));
    }

    public static Feruchemist? GetFeruchemicGeneForMetal(this Pawn_GeneTracker genes, MetalDef metal) {
        GeneDef geneDef = GeneDefOf.GetFerringGeneForMetal(metal);

        return (Feruchemist)genes.GetGene(geneDef);
    }

    public static bool HasFeruchemicGeneForMetal(this Pawn_GeneTracker genes, MetalDef metal) {
        return genes.HasActiveGene(GeneDefOf.GetFerringGeneForMetal(metal));
    }

    public static IEnumerable<Allomancer> GetAllomanticGenes(this Pawn_GeneTracker genes) {
        return genes.GenesListForReading.Where(x => x is Allomancer).Cast<Allomancer>().ToList();
    }

    public static IEnumerable<Feruchemist> GetFeruchemicGenes(this Pawn_GeneTracker genes) {
        return genes.GenesListForReading.Where(x => x is Feruchemist).Cast<Feruchemist>().ToList();
    }
}