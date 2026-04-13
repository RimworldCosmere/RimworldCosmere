using RimWorld;
using Verse;

namespace Cosmere.Core.Extension;

public static class Pawn_GeneTrackerExtension {
    public static void RemoveGene(this Pawn_GeneTracker genes, GeneDef geneDef) {
        if (!genes.HasActiveGene(geneDef)) return;

        genes.RemoveGene(genes.GetGene(geneDef));
    }

    public static Verse.Gene TryAddGene(this Pawn_GeneTracker genes, GeneDef gene, bool xenogene = false) {
        return genes.HasActiveGene(gene) ? genes.GetGene(gene) : genes.AddGene(gene, xenogene);
    }
}