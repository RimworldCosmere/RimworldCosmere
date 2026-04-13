using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Utility;

public static class MetalbornUtility {
    public static bool HasAnyMetalbornGene(Pawn? pawn) {
        if (pawn?.genes == null) return false;
        List<Verse.Gene> genes = pawn.genes.GenesListForReading;
        for (int i = 0; i < genes.Count; i++) {
            if (genes[i] is Allomancer or Feruchemist && genes[i].Active) return true;
        }
        return false;
    }

    public static bool HasAnyMistingGene(Pawn? pawn) {
        if (pawn?.genes == null) return false;
        List<Verse.Gene> genes = pawn.genes.GenesListForReading;
        for (int i = 0; i < genes.Count; i++) {
            if (genes[i] is Allomancer && genes[i].Active) return true;
        }
        return false;
    }

    public static bool HasAnyFerringGene(Pawn? pawn) {
        if (pawn?.genes == null) return false;
        List<Verse.Gene> genes = pawn.genes.GenesListForReading;
        for (int i = 0; i < genes.Count; i++) {
            if (genes[i] is Feruchemist && genes[i].Active) return true;
        }
        return false;
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
        if (pawn.genes == null) return;
        bool allomancy = traitDef == TraitDefOf.Cosmere_Scadrial_Trait_Mistborn;
        List<MetallicArtsMetalDef> allDefs = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading;
        bool hasAllGenes = true;
        for (int i = 0; i < allDefs.Count; i++) {
            MetallicArtsMetalDef metal = allDefs[i];
            bool relevant = allomancy ? metal.allomancy?.userName != null : metal.feruchemy?.userName != null;
            if (!relevant) continue;
            GeneDef? gene = allomancy ? metal.GetMistingGene() : metal.GetFerringGene();
            if (!pawn.genes.HasActiveGene(gene)) {
                hasAllGenes = false;
                break;
            }
        }

        if (!hasAllGenes) {
            Trait? existing = pawn.story.traits.GetTrait(traitDef);
            if (existing != null) pawn.story.traits.RemoveTrait(existing);
        } else if (!pawn.story.traits.HasTrait(traitDef)) {
            pawn.story.TryAddTrait(traitDef);
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