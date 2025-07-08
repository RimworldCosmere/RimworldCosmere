#nullable disable
using System.Diagnostics.CodeAnalysis;
using CosmereResources.Def;
using RimWorld;

namespace CosmereResources;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class MetalDefOf {        
    public static MetalDef Aluminum;
    public static MetalDef Atium;
    public static MetalDef Bendalloy;
    public static MetalDef Brass;
    public static MetalDef Bronze;
    public static MetalDef Cadmium;
    public static MetalDef Chromium;
    public static MetalDef Copper;
    public static MetalDef Duralumin;
    public static MetalDef Electrum;
    public static MetalDef Gold;
    public static MetalDef Harmonium;
    public static MetalDef Iron;
    public static MetalDef Lerasium;
    public static MetalDef LerasiumAlloy;
    public static MetalDef Leratium;
    public static MetalDef LeratiumAlloy;
    public static MetalDef Nickel;
    public static MetalDef Nicrosil;
    public static MetalDef Pewter;
    public static MetalDef Silver;
    public static MetalDef Steel;
    public static MetalDef Tin;
    public static MetalDef Trellium;
    public static MetalDef Zinc;

    static MetalDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(MetalDefOf));
    }
}
