using Cosmere.Core.Def;
using Cosmere.Core.Savant;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Savant;

public static class ScadrialSavantUtility {
    public static int GetAllomanticSavantStage(Pawn pawn, MetalDef metal) {
        RecordDef recordDef = RecordDefOf.GetTimeSpentBurningForMetal(metal);
        float ticks = pawn.records.GetValue(recordDef);
        return SavantUtility.GetAllomanticStage(ticks);
    }

    public static int GetFeruchemicalSavantStage(Pawn pawn, MetalDef metal) {
        RecordDef storingRecord = RecordDefOf.GetTimeSpentStoringForMetal(metal);
        RecordDef tappingRecord = RecordDefOf.GetTimeSpentTappingForMetal(metal);
        float ticks = pawn.records.GetValue(storingRecord) + pawn.records.GetValue(tappingRecord);
        return SavantUtility.GetFeruchemicalStage(ticks);
    }

    public static HediffDef? GetAllomanticSavantHediffDef(MetalDef metal) {
        return DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_AllomanticSavant_" + metal.defName);
    }

    public static HediffDef? GetAllomanticWithdrawalHediffDef(MetalDef metal) {
        return DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_AllomanticWithdrawal_" + metal.defName);
    }

    public static HediffDef? GetAllomanticPermanentHediffDef(MetalDef metal) {
        return DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_AllomanticSavantPermanent_" + metal.defName);
    }

    public static HediffDef? GetFeruchemicalSavantHediffDef(MetalDef metal) {
        return DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_FeruchemicalSavant_" + metal.defName);
    }

    public static HediffDef? GetFeruchemicalPermanentHediffDef(MetalDef metal) {
        return DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_FeruchemicalSavantPermanent_" + metal.defName);
    }
}
