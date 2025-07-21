using Cosmere.Framework.Extension;
using Cosmere.Roshar.Gene;
using RimWorld;
using Verse;

namespace Cosmere.Roshar.Extension;

public static class Pawn_GeneTrackerExtension {
    public static Surgebinder TryAddRadiantOrder(this Pawn_GeneTracker genes, GeneDef geneDef, int ideal = 0, bool xenogene = false) {
        Surgebinder gene = (Surgebinder) genes.TryAddGene(geneDef, xenogene);
        gene.currentIdeal = ideal;

        return gene;
    }
}