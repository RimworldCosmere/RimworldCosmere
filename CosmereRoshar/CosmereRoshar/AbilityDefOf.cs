#nullable disable
using System.Diagnostics.CodeAnalysis;
using Cosmere.Roshar.Def;
using RimWorld;

namespace Cosmere.Roshar;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class AbilityDefOf {
    public static SurgebindingAbilityDef Cosmere_Roshar_Ability_BreatheStormlight;

    static AbilityDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(AbilityDefOf));
    }
}