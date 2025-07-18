#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace CosmereScadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class StatDefOf {
    public static StatDef Cosmere_Scadrial_Stat_AllomanticPower;
    public static StatDef Cosmere_Scadrial_Stat_FeruchemicPower;

    static StatDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(StatDefOf));
    }
}