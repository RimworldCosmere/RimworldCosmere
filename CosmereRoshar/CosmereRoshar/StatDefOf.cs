#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace CosmereRoshar;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class StatDefOf {
    public static StatDef Cosmere_Roshar_Stat_SphereSize;

    static StatDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(StatDefOf));
    }
}