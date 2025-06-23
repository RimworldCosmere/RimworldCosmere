using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace CosmereScadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class ThingCategoryDefOf {
    public static ThingCategoryDef Cosmere_Allomantic_Vial;

    static ThingCategoryDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ThingCategoryDefOf));
    }
}