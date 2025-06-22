using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Defs;
using CosmereScadrial.Genes;
using RimWorld;
using Verse;

namespace CosmereScadrial.Utils;

public static class MetalbornUtility {
    public static bool HasAnyMetalbornGene(Pawn pawn) {
        return pawn?.genes != null && pawn.genes.GenesListForReading.Any(gene => gene is Allomancer && gene.Active);
    }

    public static void HandleMetalbornTrait(Pawn pawn) {
        if (!HasAnyMetalbornGene(pawn)) {
            Trait? trait = pawn.story.traits.GetTrait(TraitDefOf.Cosmere_Metalborn);
            if (trait != null) pawn.story.traits.RemoveTrait(trait);
            return;
        }

        if (!pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Metalborn)) {
            pawn.story.traits.GainTrait(new Trait(TraitDefOf.Cosmere_Metalborn));
        }
    }

    public static void HandleMistbornAndFullFeruchemistTraits(Pawn pawn) {
        HandleMistbornTrait(pawn);
        HandleFullFeruchemistTrait(pawn);
    }

    private static void HandleMistbornTrait(Pawn pawn) {
        HandleCombinedTrait(pawn, TraitDefOf.Cosmere_Mistborn);
    }

    private static void HandleFullFeruchemistTrait(Pawn pawn) {
        HandleCombinedTrait(pawn, TraitDefOf.Cosmere_FullFeruchemist);
    }

    private static void HandleCombinedTrait(Pawn pawn, TraitDef traitDef) {
        Trait trait = new Trait(traitDef);
        bool allomancy = traitDef.defName == TraitDefOf.Cosmere_Mistborn.defName;
        IEnumerable<string> geneStrings = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading
            .Where(x => !x.godMetal && (allomancy ? x.allomancy != null : x.feruchemy != null))
            .Select(x => allomancy ? "Cosmere_Misting_" + x.defName : "Cosmere_Ferring_" + x.defName);
        foreach (string? gene in geneStrings) {
            if (pawn.genes.HasActiveGene(DefDatabase<GeneDef>.GetNamed(gene))) continue;
            if (pawn.story.traits.HasTrait(trait.def)) pawn.story.traits.RemoveTrait(trait);

            return;
        }

        pawn.story.traits.GainTrait(trait);
        pawn.story.traits.RecalculateSuppression();
    }
}