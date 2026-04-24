#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace Cosmere.System.Scadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class XenotypeDefOf {
    [MayRequire("Cosmere.Scadrial")]
    public static XenotypeDef Cosmere_Scadrial_Xenotype_Noble;

    [MayRequire("Cosmere.Scadrial")]
    public static XenotypeDef Cosmere_Scadrial_Xenotype_Scadrian;

    [MayRequire("Cosmere.Scadrial")]
    public static XenotypeDef Cosmere_Scadrial_Xenotype_Skaa;

    [MayRequire("Cosmere.Scadrial")]
    public static XenotypeDef Cosmere_Scadrial_Xenotype_Terris;

    static XenotypeDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(XenotypeDefOf));
    }
}