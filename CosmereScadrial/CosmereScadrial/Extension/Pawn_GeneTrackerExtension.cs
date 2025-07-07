using System.Collections.Generic;
using System.Linq;
using CosmereResources.Def;
using CosmereScadrial.Gene;
using RimWorld;
using Verse;

namespace CosmereScadrial.Extension;

public static class Pawn_GeneTrackerExtension {
    public static Allomancer? GetAllomanticGeneForMetal(this Pawn_GeneTracker genes, MetalDef metal) {
        GeneDef? geneDef = metal.GetMistingGene();

        return geneDef == null ? null : (Allomancer)genes.GetGene(geneDef);
    }

    public static bool HasAllomanticGeneForMetal(this Pawn_GeneTracker genes, MetalDef metal) {
        return genes.HasActiveGene(metal.GetMistingGene());
    }

    public static Feruchemist? GetFeruchemicGeneForMetal(this Pawn_GeneTracker genes, MetalDef metal) {
        GeneDef? geneDef = metal.GetFerringGene();

        return geneDef == null ? null : (Feruchemist)genes.GetGene(geneDef);
    }

    public static bool HasFeruchemicGeneForMetal(this Pawn_GeneTracker genes, MetalDef metal) {
        return genes.HasActiveGene(metal.GetFerringGene());
    }

    public static List<Allomancer> GetAllomanticGenes(this Pawn_GeneTracker genes) {
        return genes.GenesListForReading.Where(x => x is Allomancer).Cast<Allomancer>().ToList();
    }

    public static List<Feruchemist> GetFeruchemicGenes(this Pawn_GeneTracker genes) {
        return genes.GenesListForReading.Where(x => x is Feruchemist).Cast<Feruchemist>().ToList();
    }
}