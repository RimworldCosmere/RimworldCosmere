using CosmereResources.Defs;
using CosmereScadrial.Defs;

namespace CosmereScadrial.Extensions;

public static class MetalDefExtension {
    public static MetallicArtsMetalDef GetMetallicArtsMetalDef(this MetalDef def) {
        return MetallicArtsMetalDef.GetFromMetalDef(def);
    }
}