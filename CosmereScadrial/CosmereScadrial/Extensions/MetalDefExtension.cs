using CosmereResources.Defs;
using CosmereScadrial.Defs;

namespace CosmereScadrial.Extensions;

public static class MetalDefExtension {
    public static MetallicArtsMetalDef ToMetallicArts(this MetalDef def) {
        return MetallicArtsMetalDef.FromMetalDef(def);
    }
}