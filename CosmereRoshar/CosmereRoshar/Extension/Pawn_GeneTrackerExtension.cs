using Cosmere.Resources.Def;
using Cosmere.Roshar.Def;
using Cosmere.Roshar.Gene;
using RimWorld;
using Verse;

namespace Cosmere.Roshar.Extension;

public static class Pawn_GeneTrackerExtension {
    public static Surgebinder TryAddRadiantOrder(
        this Pawn_GeneTracker genes,
        GeneDef geneDef,
        int ideal = 0,
        bool xenogene = false
    ) {
        Surgebinder gene = (Surgebinder)genes.TryAddGene(geneDef, xenogene);
        gene.currentIdeal = ideal;

        return gene;
    }

    public static Surgebinder? GetSurgebindingGeneForGem(this Pawn_GeneTracker genes, GemDef def) {
        GeneDef? geneDef = def.GetSurgebindingGene();

        return geneDef == null ? null : (Surgebinder)genes.GetGene(geneDef);
    }

    public static bool HasSurgebindingGeneForGem(this Pawn_GeneTracker genes, GemDef def) {
        return genes.HasActiveGene(def.GetSurgebindingGene());
    }

    public static Surgebinder? GetSurgebindingGeneForOrder(this Pawn_GeneTracker genes, RadiantOrderDef def) {
        GeneDef geneDef = def.GetSurgebindingGene();

        return (Surgebinder)genes.GetGene(geneDef);
    }

    public static bool HasSurgebindingGeneForOrder(this Pawn_GeneTracker genes, RadiantOrderDef def) {
        return genes.HasActiveGene(def.GetSurgebindingGene());
    }
}