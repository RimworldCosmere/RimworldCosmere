#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class HemalurgicDefOf {
    static HemalurgicDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(HemalurgicDefOf));
    }

    [MayRequire("Cosmere.Scadrial")]
    public static ThingDef Cosmere_Scadrial_Thing_HemalurgicSpike;

    [MayRequire("Cosmere.Scadrial")]
    public static ThingDef Cosmere_Scadrial_Thing_HemalurgicNeedle;

    [MayRequire("Cosmere.Scadrial")]
    public static ThingDef Cosmere_Scadrial_Thing_AluminumSpikeCase;

    [MayRequire("Cosmere.Scadrial")]
    public static HediffDef Cosmere_Scadrial_Hediff_HemalurgicSpikes;

    [MayRequire("Cosmere.Scadrial")]
    public static HediffDef Cosmere_Scadrial_Hediff_Drab;

    [MayRequire("Cosmere.Scadrial")]
    public static HediffDef Cosmere_Scadrial_Hediff_RuinsInfluence;

    [MayRequire("Cosmere.Scadrial")]
    public static ThoughtDef Cosmere_Scadrial_Thought_RuinsWhispers;

    [MayRequire("Cosmere.Scadrial")]
    public static RecipeDef Cosmere_Scadrial_Recipe_ChargeHemalurgicSpike;

    [MayRequire("Cosmere.Scadrial")]
    public static RecipeDef Cosmere_Scadrial_Recipe_ImplantHemalurgicSpike;

    [MayRequire("Cosmere.Scadrial")]
    public static RecipeDef Cosmere_Scadrial_Recipe_RemoveHemalurgicSpike;

    [MayRequire("Cosmere.Scadrial")]
    public static ResearchProjectDef Cosmere_Scadrial_HemalurgicPrecision;

    [MayRequire("Cosmere.Scadrial")]
    public static ResearchProjectDef Cosmere_Scadrial_HemalurgicMastery;
}
