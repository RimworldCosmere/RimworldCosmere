#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace Cosmere.System.Scadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class ScenarioDefOf {
    [MayRequire("Cosmere.Scadrial")]
    public static ScenarioDef Cosmere_Scadrial_Scenario_PreCatacendre;

    [MayRequire("Cosmere.Scadrial")]
    public static ScenarioDef Cosmere_Scadrial_Scenario_PostCatacendre;

    static ScenarioDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ScenarioDefOf));
    }
}