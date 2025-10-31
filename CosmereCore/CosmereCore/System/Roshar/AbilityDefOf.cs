using Cosmere;
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
public class AbilityDefOf {
    public static SurgebindingAbilityDef Cosmere_Roshar_Ability_BreatheStormlight;
    public static SurgebindingAbilityDef Cosmere_Roshar_Ability_Heal;
    public static SurgebindingAbilityDef Cosmere_Roshar_Ability_ToggleShardblade;
    public static SurgebindingAbilityDef Cosmere_Roshar_Ability_ToggleShardplate;

    static AbilityDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(AbilityDefOf));
    }
}