#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace Cosmere.System.Scadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class AbilityDefOf {
    [MayRequire("Cosmere.Scadrial")]
    public static AbilityDef Cosmere_Scadrial_Ability_BrassAura;

    [MayRequire("Cosmere.Scadrial")]
    public static AbilityDef Cosmere_Scadrial_Ability_BrassTarget;

    [MayRequire("Cosmere.Scadrial")]
    public static AbilityDef Cosmere_Scadrial_Ability_BronzeAura;

    [MayRequire("Cosmere.Scadrial")]
    public static AbilityDef Cosmere_Scadrial_Ability_CopperAura;

    [MayRequire("Cosmere.Scadrial")]
    public static AbilityDef Cosmere_Scadrial_Ability_Aluminum;

    [MayRequire("Cosmere.Scadrial")]
    public static AbilityDef Cosmere_Scadrial_Ability_Cadmium;

    [MayRequire("Cosmere.Scadrial")]
    public static AbilityDef Cosmere_Scadrial_Ability_Electrum;

    [MayRequire("Cosmere.Scadrial")]
    public static AbilityDef Cosmere_Scadrial_Ability_Atium;

    [MayRequire("Cosmere.Scadrial")]
    public static AbilityDef Cosmere_Scadrial_Ability_Bendalloy;

    [MayRequire("Cosmere.Scadrial")]
    public static AbilityDef Cosmere_Scadrial_Ability_Duralumin;

    [MayRequire("Cosmere.Scadrial")]
    public static AbilityDef Cosmere_Scadrial_Ability_Nicrosil;

    [MayRequire("Cosmere.Scadrial")]
    public static AbilityDef Cosmere_Scadrial_Ability_IronAura;

    [MayRequire("Cosmere.Scadrial")]
    public static AbilityDef Cosmere_Scadrial_Ability_IronPull;

    [MayRequire("Cosmere.Scadrial")]
    public static AbilityDef Cosmere_Scadrial_Ability_Pewter;

    [MayRequire("Cosmere.Scadrial")]
    public static AbilityDef Cosmere_Scadrial_Ability_SteelAura;

    [MayRequire("Cosmere.Scadrial")]
    public static AbilityDef Cosmere_Scadrial_Ability_SteelPush;

    [MayRequire("Cosmere.Scadrial")]
    public static AbilityDef Cosmere_Scadrial_Ability_SteelCoinshot;

    [MayRequire("Cosmere.Scadrial")]
    public static AbilityDef Cosmere_Scadrial_Ability_Tin;

    [MayRequire("Cosmere.Scadrial")]
    public static AbilityDef Cosmere_Scadrial_Ability_ZincAura;

    [MayRequire("Cosmere.Scadrial")]
    public static AbilityDef Cosmere_Scadrial_Ability_ZincTarget;

    static AbilityDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(AbilityDefOf));
    }
}