using CosmereResources.Defs;
using CosmereScadrial.Defs;
using Verse;

namespace CosmereScadrial.Extensions;

public static class MetalDefExtension {
    public static MetallicArtsMetalDef ToMetallicArts(this MetalDef def) {
        return MetallicArtsMetalDef.FromMetalDef(def);
    }

    public static ThingDef GetVial(this MetalDef def) {
        return DefDatabase<ThingDef>.GetNamed("Cosmere_Scadrial_Thing_AllomanticVial" + def.defName);
    }
}