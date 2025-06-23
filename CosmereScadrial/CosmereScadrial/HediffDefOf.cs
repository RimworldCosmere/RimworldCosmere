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
public static class HediffDefOf {
    public static HediffDef Cosmere_Hediff_Brass_Aura;
    public static HediffDef Cosmere_Hediff_Soothed;
    public static HediffDef Cosmere_Hediff_Bronze_Aura;
    public static HediffDef Cosmere_Hediff_Steel_Aura;
    public static HediffDef Cosmere_Hediff_Steel_Jump;
    public static HediffDef Cosmere_Hediff_Steel_Bubble;
    public static HediffDef Cosmere_Hediff_Iron_Aura;
    public static HediffDef Cosmere_Hediff_Zinc_Aura;
    public static HediffDef Cosmere_Hediff_Rioted_Buff;
    public static HediffDef Cosmere_Hediff_Rioted_Debuff;
    public static HediffDef Cosmere_Hediff_Copper_Cloud;
    public static HediffDef Cosmere_Hediff_Copper_Aura;
    public static HediffDef Cosmere_Hediff_Pewter_Buff;
    public static HediffDef Cosmere_Hediff_Pewter_Drag;
    public static HediffDef Cosmere_Hediff_Tin_Buff;
    public static HediffDef Cosmere_Hediff_Tin_Drag;
    public static HediffDef Cosmere_Hediff_Surge_Charge;
    public static HediffDef Cosmere_Hediff_Investiture_Shield;
    public static HediffDef Cosmere_Scadrial_Mist_Coma;
    public static HediffDef Cosmere_Hediff_Time_Bubble_Cadmium;
    public static HediffDef Cosmere_Hediff_Time_Bubble_Bendalloy;
    public static HediffDef Cosmere_Hediff_Atium_Buff;
    public static HediffDef Cosmere_Hediff_Electrum_Buff;
    public static HediffDef Cosmere_Hediff_Gold;
    public static HediffDef Cosmere_Hediff_Post_Gold;

    static HediffDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
    }
}