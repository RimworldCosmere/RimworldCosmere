using CosmereResources.Def;
using CosmereScadrial.Util;
using RimWorld;
using Verse;

namespace CosmereScadrial.Extension;

public static class PawnExtension {
    public static bool IsMistborn(this Pawn pawn) {
        Need_Food? need = pawn.needs.food;

        return pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Scadrial_Trait_Mistborn);
    }

    public static bool IsFullFeruchemist(this Pawn pawn) {
        return pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Scadrial_Trait_FullFeruchemist);
    }

    public static bool IsAllomancer(this Pawn pawn) {
        return pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Scadrial_Trait_Allomancer);
    }

    public static bool IsFeruchemist(this Pawn pawn) {
        return pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Scadrial_Trait_Feruchemist);
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

    public static bool IsCompounding(this Pawn pawn, MetalDef metal) {
        return pawn.health.hediffSet.HasHediff(
            DefDatabase<HediffDef>.GetNamed("Cosmere_Scadrial_Hediff_Compound" + metal.defName, false)
        );
    }
}