#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace CosmereScadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static partial class ThingDefOf {
    public static ThingDef Cosmere_Scadrial_Mote_CopperCloud;
    public static ThingDef Cosmere_Scadrial_Thing_TimeBubbleCadmium;
    public static ThingDef Cosmere_Scadrial_Thing_TimeBubbleBendalloy;
    public static ThingDef Cosmere_Scadrial_Thing_TimeBubbleWarp;

    static ThingDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
    }
}