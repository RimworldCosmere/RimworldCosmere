#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace CosmereScadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class ScenarioDefOf {
    public static ScenarioDef Cosmere_Scadrial_PreCatacendre;
    public static ScenarioDef Cosmere_Scadrial_PostCatacendre;

    static ScenarioDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ScenarioDefOf));
    }
}