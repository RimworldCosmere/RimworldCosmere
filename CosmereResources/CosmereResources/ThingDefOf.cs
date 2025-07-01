using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace CosmereResources;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static partial class ThingDefOf {
    public static ThingDef Coal;
    public static ThingDef Charcoal;
    public static ThingDef Cosmere_Resource_Thing_Alcohol;
    public static ThingDef Cosmere_Resource_Thing_Glass;

    static ThingDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
    }
}