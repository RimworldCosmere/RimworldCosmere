#nullable disable
using System.Diagnostics.CodeAnalysis;
using Cosmere.System.Roshar.Def;
using RimWorld;

namespace Cosmere.System.Roshar;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class AbilityDefOf {
    [MayRequire("Cosmere.Roshar")]
    public static SurgebindingAbilityDef Cosmere_Roshar_Ability_BreatheStormlight;

    [MayRequire("Cosmere.Roshar")]
    public static SurgebindingAbilityDef Cosmere_Roshar_Ability_Heal;

    [MayRequire("Cosmere.Roshar")]
    public static SurgebindingAbilityDef Cosmere_Roshar_Ability_ToggleShardblade;

    [MayRequire("Cosmere.Roshar")]
    public static SurgebindingAbilityDef Cosmere_Roshar_Ability_ToggleShardplate;

    [MayRequire("Cosmere.Roshar")]
    public static SurgebindingAbilityDef Cosmere_Roshar_Ability_Invisibility;

    [MayRequire("Cosmere.Roshar")]
    public static AbilityDef Cosmere_Roshar_Ability_HonorsPerpendicularity;

    [MayRequire("Cosmere.Roshar")]
    public static AbilityDef Cosmere_Roshar_Ability_CultivationsPerpendicularity;

    [MayRequire("Cosmere.Roshar")]
    public static AbilityDef Cosmere_Roshar_Ability_SiblingBlessing;

    [MayRequire("Cosmere.Roshar")]
    public static AbilityDef Cosmere_Roshar_Ability_ReinforceStructure;

    static AbilityDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(AbilityDefOf));
    }
}
