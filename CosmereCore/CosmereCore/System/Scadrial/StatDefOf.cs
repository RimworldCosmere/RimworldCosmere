#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace Cosmere.System.Scadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class StatDefOf {
    [MayRequire("Cosmere.Scadrial")]
    public static StatDef Cosmere_Scadrial_Stat_AllomanticPower;

    [MayRequire("Cosmere.Scadrial")]
    public static StatDef Cosmere_Scadrial_Stat_FeruchemicPower;

    static StatDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(StatDefOf));
    }
}