#nullable disable
using System.Diagnostics.CodeAnalysis;
using Cosmere.Def;
using RimWorld;

namespace Cosmere;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class GemDefOf {        
    public static GemDef Amethyst;
    public static GemDef Diamond;
    public static GemDef Emerald;
    public static GemDef Garnet;
    public static GemDef Heliodor;
    public static GemDef Ruby;
    public static GemDef Sapphire;
    public static GemDef Smokestone;
    public static GemDef Topaz;
    public static GemDef Zircon;

    static GemDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(GemDefOf));
    }
}
