#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace CosmereScadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class RecordDefOf {
    public static RecordDef Cosmere_IngestedLerasium;
    public static RecordDef Cosmere_IngestedLerasiumAlloy;
    public static RecordDef Cosmere_IngestedLeratium;
    public static RecordDef Cosmere_IngestedLeratiumAlloy;
    public static RecordDef Cosmere_IngestedVial;
    public static RecordDef Cosmere_IngestedRawMetal;

    static RecordDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(RecordDefOf));
    }
}