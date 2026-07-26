#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace Cosmere.System.Roshar;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class XenotypeDefOf {
    [MayRequire("Cosmere.Roshar")]
    public static XenotypeDef Cosmere_Roshar_Xenotype_Lighteyes;

    [MayRequire("Cosmere.Roshar")]
    public static XenotypeDef Cosmere_Roshar_Xenotype_Darkeyes;

    static XenotypeDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(XenotypeDefOf));
    }
}