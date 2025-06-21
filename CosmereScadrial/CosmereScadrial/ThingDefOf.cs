using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace CosmereScadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class ThingDefOf {
    public static ThingDef Cosmere_Mote_Copper_Cloud;
    public static ThingDef Cosmere_Scadrial_TimeBubble_Cadmium;
    public static ThingDef Cosmere_Scadrial_TimeBubble_Bendalloy;
    public static ThingDef Cosmere_Scadrial_TimeBubble_Warp;

    static ThingDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
    }
}