#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace CosmereCore;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class ThingCategoryDefOf {
    public static ThingCategoryDef Cosmere;
    public static ThingCategoryDef Cosmere_Currency;

    static ThingCategoryDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ThingCategoryDefOf));
    }
}