using CosmereResources.Def;
using CosmereScadrial.Def;
using RimWorld;
using Verse;

namespace CosmereScadrial.Extension;

public static class MetalDefExtension {
    public static MetallicArtsMetalDef ToMetallicArts(this MetalDef def) {
        if (def is MetallicArtsMetalDef metallicArtsMetal) return metallicArtsMetal;

        return MetallicArtsMetalDef.FromMetalDef(def);
    }

    public static GeneDef? GetMistingGene(this MetalDef def) {
        return GeneDefOf.GetMistingGeneForMetal(def);
    }

    public static GeneDef? GetFerringGene(this MetalDef def) {
        return GeneDefOf.GetFerringGeneForMetal(def);
    }

    public static TraitDef? GetMistingTrait(this MetalDef def) {
        return TraitDefOf.GetMistingTraitForMetal(def);
    }

    public static TraitDef? GetFerringTrait(this MetalDef def) {
        return TraitDefOf.GetFerringTraitForMetal(def);
    }

    public static AllomanticAbilityDef? GetCompoundAbility(this MetalDef def) {
        AbilityDef? ability =
            DefDatabase<AbilityDef>.GetNamedSilentFail("Cosmere_Scadrial_Ability_Compound" + def.defName);

        return (AllomanticAbilityDef)ability ?? null;
    }
}