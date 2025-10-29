#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class ThingCategoryDefOf {
    public static ThingCategoryDef Cosmere_Scadrial_ThingCategory_Allomancy;
    public static ThingCategoryDef Cosmere_Scadrial_ThingCategory_Feruchemy;

    static ThingCategoryDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ThingCategoryDefOf));
    }
}