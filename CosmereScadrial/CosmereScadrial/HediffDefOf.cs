#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace CosmereScadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static partial class HediffDefOf {
    public static HediffDef Cosmere_Scadrial_Hediff_MistComa;

    static HediffDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
    }
}