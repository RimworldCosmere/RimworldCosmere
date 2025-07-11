#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace CosmereScadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class AbilityDefOf {
    public static AbilityDef Cosmere_Scadrial_Ability_BrassAura;
    public static AbilityDef Cosmere_Scadrial_Ability_BrassTarget;
    public static AbilityDef Cosmere_Scadrial_Ability_BronzeAura;
    public static AbilityDef Cosmere_Scadrial_Ability_CopperAura;
    public static AbilityDef Cosmere_Scadrial_Ability_Aluminum;
    public static AbilityDef Cosmere_Scadrial_Ability_Cadmium;
    public static AbilityDef Cosmere_Scadrial_Ability_Electrum;
    public static AbilityDef Cosmere_Scadrial_Ability_Atium;
    public static AbilityDef Cosmere_Scadrial_Ability_Bendalloy;
    public static AbilityDef Cosmere_Scadrial_Ability_Duralumin;
    public static AbilityDef Cosmere_Scadrial_Ability_Nicrosil;
    public static AbilityDef Cosmere_Scadrial_Ability_IronAura;
    public static AbilityDef Cosmere_Scadrial_Ability_IronPull;
    public static AbilityDef Cosmere_Scadrial_Ability_Pewter;
    public static AbilityDef Cosmere_Scadrial_Ability_SteelAura;
    public static AbilityDef Cosmere_Scadrial_Ability_SteelPush;
    public static AbilityDef Cosmere_Scadrial_Ability_SteelCoinshot;
    public static AbilityDef Cosmere_Scadrial_Ability_Tin;
    public static AbilityDef Cosmere_Scadrial_Ability_ZincAura;
    public static AbilityDef Cosmere_Scadrial_Ability_ZincTarget;

    static AbilityDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(AbilityDefOf));
    }
}