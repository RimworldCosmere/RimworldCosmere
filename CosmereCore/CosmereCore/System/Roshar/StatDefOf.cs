#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace Cosmere.System.Roshar;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class StatDefOf {
    static StatDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(StatDefOf));
    }

    [MayRequire("Cosmere.Roshar")]
    public static StatDef Cosmere_Roshar_Stat_SphereSize;
}