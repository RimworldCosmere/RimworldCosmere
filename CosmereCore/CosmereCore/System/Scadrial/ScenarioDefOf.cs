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
    static ScenarioDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ScenarioDefOf));
    }

    [MayRequire("Cosmere.Scadrial")]
    public static ScenarioDef Cosmere_Scadrial_PreCatacendre;

    [MayRequire("Cosmere.Scadrial")]
    public static ScenarioDef Cosmere_Scadrial_PostCatacendre;
}