#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace CosmereResources;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class ThingCategoryDefOf {
    public static ThingCategoryDef Cosmere_Resource_ThingCategory_Metal;
    public static ThingCategoryDef Cosmere_Resource_ThingCategory_BasicMetal;
    public static ThingCategoryDef Cosmere_Resource_ThingCategory_GodMetal;

    static ThingCategoryDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ThingCategoryDefOf));
    }
}