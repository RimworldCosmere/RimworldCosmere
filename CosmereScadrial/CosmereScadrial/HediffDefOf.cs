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
    public static HediffDef Cosmere_Hediff_BrassAura;
    public static HediffDef Cosmere_Hediff_Soothed;
    public static HediffDef Cosmere_Hediff_BronzeAura;
    public static HediffDef Cosmere_Hediff_SteelAura;
    public static HediffDef Cosmere_Hediff_SteelBubble;
    public static HediffDef Cosmere_Hediff_IronAura;
    public static HediffDef Cosmere_Hediff_ZincAura;
    public static HediffDef Cosmere_Hediff_Rioted_Buff;
    public static HediffDef Cosmere_Hediff_Rioted_Debuff;
    public static HediffDef Cosmere_Hediff_Copperclouded;
    public static HediffDef Cosmere_Hediff_CopperAura;
    public static HediffDef Cosmere_Hediff_PewterBuff;
    public static HediffDef Cosmere_Hediff_TinBuff;
    public static HediffDef Cosmere_Hediff_PewterDrag;
    public static HediffDef Cosmere_Hediff_TinDrag;
    public static HediffDef Cosmere_Hediff_Surge_Charge;
    public static HediffDef Cosmere_Hediff_Investiture_Shield;
    public static HediffDef Cosmere_Scadrial_MistComa;
    public static HediffDef? Cosmere_Hediff_TimeBubble_Cadmium;
    public static HediffDef? Cosmere_Hediff_TimeBubble_Bendalloy;
    public static HediffDef Cosmere_Hediff_AtiumBuff;
    public static HediffDef Cosmere_Hediff_ElectrumBuff;
    public static HediffDef Cosmere_Hediff_Gold;
    public static HediffDef Cosmere_Hediff_PostGold;

    static HediffDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
    }
}