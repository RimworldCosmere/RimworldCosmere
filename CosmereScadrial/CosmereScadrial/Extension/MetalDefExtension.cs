using CosmereResources.Def;
using CosmereScadrial.Def;
using Verse;

namespace CosmereScadrial.Extension;

public static class MetalDefExtension {
    public static MetallicArtsMetalDef ToMetallicArts(this MetalDef def) {
        if (def is MetallicArtsMetalDef metallicArtsMetal) return metallicArtsMetal;

        return MetallicArtsMetalDef.FromMetalDef(def);
    }

    public static ThingDef GetVial(this MetalDef def) {
        return DefDatabase<ThingDef>.GetNamed("Cosmere_Scadrial_Thing_AllomanticVial" + def.defName);
    }

    public static GeneDef? GetMistingGene(this MetalDef def) {
        return GeneDefOf.GetMistingGeneForMetal(def);
    }

    public static GeneDef? GetFerringGene(this MetalDef def) {
        return GeneDefOf.GetFerringGeneForMetal(def);
    }
}