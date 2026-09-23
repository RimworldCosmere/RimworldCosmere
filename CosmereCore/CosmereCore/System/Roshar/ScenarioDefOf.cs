#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace Cosmere.System.Roshar;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class ScenarioDefOf {
    [MayRequire("Cosmere.Roshar")]
    public static ScenarioDef Cosmere_Roshar_Scenario_TrueDesolation;

    static ScenarioDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ScenarioDefOf));
    }
}
