using RimWorld;
using Verse;

namespace CosmereFramework.Extension;

public static class Pawn_GeneTrackerExtension {
    public static void RemoveGene(this Pawn_GeneTracker genes, GeneDef geneDef) {
        if (!genes.HasActiveGene(geneDef)) return;

        genes.RemoveGene(genes.GetGene(geneDef));
    }

    public static Gene TryAddGene(this Pawn_GeneTracker genes, GeneDef gene, bool xenogene = false) {
        return genes.HasActiveGene(gene) ? genes.GetGene(gene) : genes.AddGene(gene, xenogene);
    }

    public static Trait TryAddTrait(Pawn pawn, TraitDef traitDef, int degree = 0, bool forced = false) {
        Trait trait;
        if (pawn.story.traits.HasTrait(traitDef)) {
            trait = pawn.story.traits.GetTrait(traitDef);
            if (trait.Degree == degree) return trait;
            pawn.story.traits.RemoveTrait(trait);
        }

        trait = new Trait(traitDef, degree, forced);
        pawn.story.traits.GainTrait(trait);

        return trait;
    }
}