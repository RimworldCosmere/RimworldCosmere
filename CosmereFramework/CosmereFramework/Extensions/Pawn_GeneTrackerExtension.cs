using RimWorld;
using Verse;

namespace CosmereFramework.Extensions;

public static class Pawn_GeneTrackerExtension {
    public static void RemoveGene(this Pawn_GeneTracker genes, GeneDef geneDef) {
        if (!genes.HasActiveGene(geneDef)) return;

        genes.RemoveGene(genes.GetGene(geneDef));
    }
}