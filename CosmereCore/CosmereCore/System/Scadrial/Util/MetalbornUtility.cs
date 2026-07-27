using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Util;

public static class MetalbornUtility {
    public static bool HasAnyMetalbornGene(Pawn? pawn) {
        return HasAnyActiveGene<Allomancer>(pawn) || HasAnyActiveGene<Feruchemist>(pawn);
    }

    public static bool HasAnyMistingGene(Pawn? pawn) {
        return HasAnyActiveGene<Allomancer>(pawn);
    }

    public static bool HasAnyFerringGene(Pawn? pawn) {
        return HasAnyActiveGene<Feruchemist>(pawn);
    }

    private static bool HasAnyActiveGene<TGene>(Pawn? pawn)
        where TGene : Verse.Gene {
        if (pawn?.genes == null) return false;
        List<Verse.Gene> genes = pawn.genes.GenesListForReading;
        for (int i = 0; i < genes.Count; i++) {
            if (genes[i] is TGene && genes[i].Active) return true;
        }

        return false;
    }

    public static void SyncMetalbornTrait(Pawn pawn) {
        HandleConditionalTrait(pawn, HasAnyMetalbornGene(pawn), TraitDefOf.Cosmere_Scadrial_Trait_Metalborn);
    }

    public static void SyncMistbornAndFullFeruchemistTraits(Pawn pawn) {
        SyncMistbornTrait(pawn);
        SyncFullFeruchemistTrait(pawn);
    }

    public static void SyncMistbornTrait(Pawn pawn) {
        HandleCombinedTrait(pawn, TraitDefOf.Cosmere_Scadrial_Trait_Mistborn);
    }

    public static void SyncFullFeruchemistTrait(Pawn pawn) {
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
            pawn.story.EnsureTrait(traitDef);
        }
    }

    public static void SyncAllomancerTrait(Pawn pawn) {
        HandleConditionalTrait(
            pawn,
            HasAnyMistingGene(pawn),
            TraitDefOf.Cosmere_Scadrial_Trait_Allomancer,
            SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower
        );
    }

    public static void SyncFeruchemistTrait(Pawn pawn) {
        HandleConditionalTrait(
            pawn,
            HasAnyFerringGene(pawn),
            TraitDefOf.Cosmere_Scadrial_Trait_Feruchemist,
            SkillDefOf.Cosmere_Scadrial_Skill_FeruchemicPower
        );
    }

    private static void HandleConditionalTrait(
        Pawn pawn,
        bool hasGene,
        TraitDef traitDef,
        SkillDef? resetSkill = null
    ) {
        if (!hasGene) {
            Trait? existing = pawn.story.traits.GetTrait(traitDef);
            if (existing != null) pawn.story.traits.RemoveTrait(existing);
            if (resetSkill != null) {
                pawn.skills.GetSkill(resetSkill).Level = 0;
            }

            return;
        }

        if (!pawn.story.traits.HasTrait(traitDef)) {
            pawn.story.traits.GainTrait(new Trait(traitDef));
        }
    }
}
