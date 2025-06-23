#nullable disable
using System.Diagnostics.CodeAnalysis;
using CosmereResources.Defs;
using CosmereScadrial.Defs;
using CosmereScadrial.Extensions;
using RimWorld;
using Verse;

namespace CosmereScadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static partial class ThingDefOf {
    public static ThingDef Cosmere_Mote_Copper_Cloud;
    public static ThingDef Cosmere_Scadrial_TimeBubble_Cadmium;
    public static ThingDef Cosmere_Scadrial_TimeBubble_Bendalloy;
    public static ThingDef Cosmere_Scadrial_TimeBubble_Warp;

    static ThingDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
    }

    public static ThingDef GetVialForMetal(MetalDef def) {
        return GetVialForMetal(def.GetMetallicArtsMetalDef());
    }

    public static ThingDef GetVialForMetal(MetallicArtsMetalDef def) {
        return DefDatabase<ThingDef>.GetNamed("Cosmere_Scadrial_Thing_AllomanticVial" + def.defName);
    }
}