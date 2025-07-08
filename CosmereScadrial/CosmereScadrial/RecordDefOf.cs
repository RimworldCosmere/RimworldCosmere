#nullable disable
using System.Diagnostics.CodeAnalysis;
using CosmereResources.Def;
using RimWorld;
using Verse;

namespace CosmereScadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static partial class RecordDefOf {
    public static RecordDef Cosmere_Scadrial_Record_IngestedLerasium;
    public static RecordDef Cosmere_Scadrial_Record_IngestedLerasiumAlloy;
    public static RecordDef Cosmere_Scadrial_Record_IngestedLeratium;
    public static RecordDef Cosmere_Scadrial_Record_IngestedLeratiumAlloy;
    public static RecordDef Cosmere_Scadrial_Record_IngestedVial;
    public static RecordDef Cosmere_Scadrial_Record_IngestedRawMetal;

    static RecordDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(RecordDefOf));
    }

    public static RecordDef GetMetalBurnRecordForMetal(MetalDef metal) {
        return DefDatabase<RecordDef>.GetNamed("Cosmere_Scadrial_Record_MetalBurnt_" + metal.defName);
    }

    public static RecordDef GetTimeSpentBurningForMetal(MetalDef metal) {
        return DefDatabase<RecordDef>.GetNamed("Cosmere_Scadrial_Record_TimeSpentBurning_" + metal.defName);
    }

    public static RecordDef GetTimeSpentStoringForMetal(MetalDef metal) {
        return DefDatabase<RecordDef>.GetNamed("Cosmere_Scadrial_Record_TimeSpentStoring_" + metal.defName);
    }

    public static RecordDef GetTimeSpentTappingForMetal(MetalDef metal) {
        return DefDatabase<RecordDef>.GetNamed("Cosmere_Scadrial_Record_TimeSpentTapping_" + metal.defName);
    }
}