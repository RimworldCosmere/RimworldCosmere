#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace Cosmere.Core;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class StuffCategoryDefOf {
    public static StuffCategoryDef Cosmere_Core_StuffCategory_Gems;
    public static StuffCategoryDef Cosmere_Core_StuffCategory_RawGems;
    public static StuffCategoryDef Cosmere_Core_StuffCategory_CutGems;

    static StuffCategoryDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(StuffCategoryDefOf));
    }
}
