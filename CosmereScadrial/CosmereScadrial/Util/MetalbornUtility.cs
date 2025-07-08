using System.Collections.Generic;
using System.Linq;
using CosmereFramework.Extension;
using CosmereScadrial.Def;
using CosmereScadrial.Extension;
using CosmereScadrial.Gene;
using RimWorld;
using Verse;

namespace CosmereScadrial.Util;

public static class MetalbornUtility {
    public static bool HasAnyMetalbornGene(Pawn? pawn) {
        return pawn?.genes != null &&
               pawn.genes.GenesListForReading.Any(gene => gene is Allomancer or Feruchemist && gene.Active);
    }

    public static bool HasAnyMistingGene(Pawn? pawn) {
        return pawn?.genes != null &&
               pawn.genes.GenesListForReading.Any(gene => gene is Allomancer && gene.Active);
    }

    public static bool HasAnyFerringGene(Pawn? pawn) {
        return pawn?.genes != null &&
               pawn.genes.GenesListForReading.Any(gene => gene is Feruchemist && gene.Active);
    }

    public static void HandleMetalbornTrait(Pawn pawn) {
        if (!HasAnyMetalbornGene(pawn)) {
            Trait? trait = pawn.story.traits.GetTrait(TraitDefOf.Cosmere_Scadrial_Trait_Metalborn);
            if (trait != null) pawn.story.traits.RemoveTrait(trait);
            return;
        }

        if (!pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Scadrial_Trait_Metalborn)) {
            pawn.story.traits.GainTrait(new Trait(TraitDefOf.Cosmere_Scadrial_Trait_Metalborn));
        }
    }

    public static void HandleMistbornAndFullFeruchemistTraits(Pawn pawn) {
        HandleMistbornTrait(pawn);
        HandleFullFeruchemistTrait(pawn);
    }

    public static void HandleMistbornTrait(Pawn pawn) {
        HandleCombinedTrait(pawn, TraitDefOf.Cosmere_Scadrial_Trait_Mistborn);
    }

    public static void HandleFullFeruchemistTrait(Pawn pawn) {
        HandleCombinedTrait(pawn, TraitDefOf.Cosmere_Scadrial_Trait_FullFeruchemist);
    }

    private static void HandleCombinedTrait(Pawn pawn, TraitDef traitDef) {
        Trait trait = new Trait(traitDef);
        bool allomancy = traitDef.defName == TraitDefOf.Cosmere_Scadrial_Trait_Mistborn.defName;
        List<MetallicArtsMetalDef> metals = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading
            .Where(x => allomancy ? x.allomancy?.userName != null : x.feruchemy?.userName != null)
            .ToList();

        bool hasAllGenes = metals.All(x =>
            pawn.genes.HasActiveGene(
                allomancy ? x.GetMistingGene() : x.GetFerringGene()
            )
        );

        switch (hasAllGenes) {
            case false when pawn.story.traits.HasTrait(trait.def):
                pawn.story.traits.RemoveTrait(trait);
                return;
            case true when !pawn.story.traits.HasTrait(traitDef): {
                pawn.story.TryAddTrait(trait.def);
                return;
            }
        }
    }

    public static void HandleAllomancerTrait(Pawn pawn) {
        if (!HasAnyMistingGene(pawn)) {
            Trait? trait = pawn.story.traits.GetTrait(TraitDefOf.Cosmere_Scadrial_Trait_Allomancer);
            if (trait != null) pawn.story.traits.RemoveTrait(trait);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower).Level = 0;
            return;
        }

        if (!pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Scadrial_Trait_Allomancer)) {
            pawn.story.traits.GainTrait(new Trait(TraitDefOf.Cosmere_Scadrial_Trait_Allomancer));
        }
    }

    public static void HandleFeruchemistTrait(Pawn pawn) {
        if (!HasAnyFerringGene(pawn)) {
            Trait? trait = pawn.story.traits.GetTrait(TraitDefOf.Cosmere_Scadrial_Trait_Feruchemist);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_FeruchemicPower).Level = 0;
            if (trait != null) pawn.story.traits.RemoveTrait(trait);
            return;
        }

        if (!pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Scadrial_Trait_Feruchemist)) {
            pawn.story.traits.GainTrait(new Trait(TraitDefOf.Cosmere_Scadrial_Trait_Feruchemist));
        }
    }
}