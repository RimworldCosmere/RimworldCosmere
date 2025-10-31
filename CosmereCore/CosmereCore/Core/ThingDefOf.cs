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
public static partial class ThingDefOf {
    public static ThingDef Cosmere_Core_Mote_InvestitureGlow;

    // From Resources
    public static ThingDef Coal;
    public static ThingDef Charcoal;
    public static ThingDef CutGem;
    public static ThingDef Cosmere_Core_Thing_Alcohol;
    public static ThingDef Cosmere_Core_Thing_Glass;

    public static ThingDef Cosmere_Core_Table_Forge;
    public static ThingDef Cosmere_Core_Table_GemCutter;
    public static ThingDef Cosmere_Core_Table_AlloyMaker;

    static ThingDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
    }
}