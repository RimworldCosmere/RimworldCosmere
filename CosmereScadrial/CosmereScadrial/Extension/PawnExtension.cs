using CosmereResources.Def;
using CosmereScadrial.Util;
using Verse;

namespace CosmereScadrial.Extension;

public static class PawnExtension {
    public static bool IsMistborn(this Pawn pawn) {
        return pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Scadrial_Trait_Mistborn);
    }

    public static bool IsAllomancer(this Pawn pawn) {
        return pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Scadrial_Trait_Allomancer);
    }

    public static bool IsMisting(this Pawn pawn, MetalDef metal) {
        return pawn.genes.HasAllomanticGeneForMetal(metal);
    }

    public static bool IsFerring(this Pawn pawn, MetalDef metal) {
        return pawn.genes.HasFeruchemicGeneForMetal(metal);
    }

    public static bool IsBurning(this Pawn pawn, MetalDef metal) {
        return AllomancyUtility.IsBurning(pawn, metal.ToMetallicArts());
    }
}