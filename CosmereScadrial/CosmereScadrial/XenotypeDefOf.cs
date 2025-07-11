#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace CosmereScadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class XenotypeDefOf {
    public static XenotypeDef Cosmere_Scadrial_Xenotype_Noble;
    public static XenotypeDef Cosmere_Scadrial_Xenotype_Scadrian;
    public static XenotypeDef Cosmere_Scadrial_Xenotype_Skaa;
    public static XenotypeDef Cosmere_Scadrial_Xenotype_Terris;

    static XenotypeDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(XenotypeDefOf));
    }
}