using RimWorld;
using Verse;

namespace CosmereFramework.Utils;

public static class PawnUtility {
    public static bool IsAsleep(Pawn pawn) {
        return pawn.CurJob?.def == JobDefOf.LayDown && pawn.jobs.curDriver is JobDriver_LayDown driver &&
               driver.asleep;
    }

    public static float DistanceBetween(Pawn pawn1, Pawn pawn2) {
        return pawn1.Position.DistanceTo(pawn2.Position);
    }

    public static Gene TryAddGene(Pawn pawn, GeneDef gene, bool xenogene = false) {
        return pawn.genes.HasActiveGene(gene) ? pawn.genes.GetGene(gene) : pawn.genes.AddGene(gene, xenogene);
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