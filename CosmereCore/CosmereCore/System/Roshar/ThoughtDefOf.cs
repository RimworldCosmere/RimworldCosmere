#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace Cosmere.System.Roshar;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class ThoughtDefOf {
    static ThoughtDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ThoughtDefOf));
    }

    [MayRequire("Cosmere.Roshar")]
    public static ThoughtDef Cosmere_Roshar_Thought_OathSpoken;

    [MayRequire("Cosmere.Roshar")]
    public static ThoughtDef Cosmere_Roshar_Thought_WitnessedOathPositive;

    [MayRequire("Cosmere.Roshar")]
    public static ThoughtDef Cosmere_Roshar_Thought_WitnessedOathJealous;

    [MayRequire("Cosmere.Roshar")]
    public static ThoughtDef Cosmere_Roshar_Thought_BrokenBond;
}
