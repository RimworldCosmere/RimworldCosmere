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
public static partial class ThingDefOf {
    static ThingDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
    }

    [MayRequire("Cosmere.Scadrial")]
    public static ThingDef Cosmere_Scadrial_Mote_CopperCloud;

    [MayRequire("Cosmere.Scadrial")]
    public static ThingDef Cosmere_Scadrial_Mote_BrassAura;

    [MayRequire("Cosmere.Scadrial")]
    public static ThingDef Cosmere_Scadrial_Mote_BronzeAura;

    [MayRequire("Cosmere.Scadrial")]
    public static ThingDef Cosmere_Scadrial_Mote_ZincAura;

    [MayRequire("Cosmere.Scadrial")]
    public static ThingDef Cosmere_Scadrial_Thing_TimeBubbleCadmium;

    [MayRequire("Cosmere.Scadrial")]
    public static ThingDef Cosmere_Scadrial_Thing_TimeBubbleBendalloy;

    [MayRequire("Cosmere.Scadrial")]
    public static ThingDef Cosmere_Scadrial_Thing_TimeBubbleWarp;

    [MayRequire("Cosmere.Scadrial")]
    public static ThingDef Cosmere_Scadrial_Thing_MetalmindEarring;

    [MayRequire("Cosmere.Scadrial")]
    public static ThingDef Cosmere_Scadrial_Thing_MetalmindBracelet;

    [MayRequire("Cosmere.Scadrial")]
    public static ThingDef Cosmere_Scadrial_Thing_MetalmindBand;

    [MayRequire("Cosmere.Scadrial")]
    public static ThingDef Cosmere_Scadrial_Thing_AllomanticVial;
}