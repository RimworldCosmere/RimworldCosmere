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
    public static HediffDef Cosmere_Scadrial_Hediff_BrassAura;
    public static HediffDef Cosmere_Scadrial_Hediff_Soothed;
    public static HediffDef Cosmere_Scadrial_Hediff_BronzeAura;
    public static HediffDef Cosmere_Scadrial_Hediff_SteelAura;
    public static HediffDef Cosmere_Scadrial_Hediff_SteelJump;
    public static HediffDef Cosmere_Scadrial_Hediff_SteelBubble;
    public static HediffDef Cosmere_Scadrial_Hediff_IronAura;
    public static HediffDef Cosmere_Scadrial_Hediff_ZincAura;
    public static HediffDef Cosmere_Scadrial_Hediff_RiotedBuff;
    public static HediffDef Cosmere_Scadrial_Hediff_RiotedDebuff;
    public static HediffDef Cosmere_Scadrial_Hediff_CopperCloud;
    public static HediffDef Cosmere_Scadrial_Hediff_CopperAura;
    public static HediffDef Cosmere_Scadrial_Hediff_PewterBuff;
    public static HediffDef Cosmere_Scadrial_Hediff_PewterDrag;
    public static HediffDef Cosmere_Scadrial_Hediff_TinBuff;
    public static HediffDef Cosmere_Scadrial_Hediff_TinDrag;
    public static HediffDef Cosmere_Scadrial_Hediff_SurgeCharge;
    public static HediffDef Cosmere_Scadrial_Hediff_InvestitureShield;
    public static HediffDef Cosmere_Scadrial_Hediff_MistComa;
    public static HediffDef Cosmere_Scadrial_Hediff_TimeBubbleCadmium;
    public static HediffDef Cosmere_Scadrial_Hediff_TimeBubbleBendalloy;
    public static HediffDef Cosmere_Scadrial_Hediff_AtiumBuff;
    public static HediffDef Cosmere_Scadrial_Hediff_ElectrumBuff;
    public static HediffDef Cosmere_Scadrial_Hediff_Gold;
    public static HediffDef Cosmere_Scadrial_Hediff_PostGold;

    static HediffDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
    }
}