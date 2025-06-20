#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace CosmereScadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static partial class TraitDefOf {
    public static TraitDef Cosmere_Metalborn;
    public static TraitDef Cosmere_Allomancer;
    public static TraitDef Cosmere_Feruchemist;
    public static TraitDef Cosmere_Mistborn;
    public static TraitDef Cosmere_FullFeruchemist;

    static TraitDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(TraitDefOf));
    }
}