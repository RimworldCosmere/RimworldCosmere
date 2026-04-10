using System.Collections.Generic;
using Cosmere.Core.Def;
using RimWorld;
using Verse;

namespace Cosmere.Core.Savant;

public static class SavantUtility {
    public const int AllomancyStage1Ticks = 180000;
    public const int AllomancyStage2Ticks = 600000;
    public const int AllomancyStage3Ticks = 1800000;

    public const int FeruchemyStage1Ticks = 300000;
    public const int FeruchemyStage2Ticks = 900000;
    public const int FeruchemyStage3Ticks = 2400000;

    public const int SurgebindingStage1Ticks = 420000;
    public const int SurgebindingStage2Ticks = 1200000;
    public const int SurgebindingStage3Ticks = 3000000;

    public const float Stage1PowerAllomancy = 1.20f;
    public const float Stage2PowerAllomancy = 1.40f;
    public const float Stage3PowerAllomancy = 1.75f;

    public const float Stage1PowerFeruchemy = 1.15f;
    public const float Stage2PowerFeruchemy = 1.35f;
    public const float Stage3PowerFeruchemy = 1.60f;

    public const float Stage1PowerSurgebinding = 1.15f;
    public const float Stage2PowerSurgebinding = 1.30f;
    public const float Stage3PowerSurgebinding = 1.50f;

    public const float Stage1CostSurgebinding = 0.90f;
    public const float Stage2CostSurgebinding = 0.80f;
    public const float Stage3CostSurgebinding = 0.70f;

    public const float Stage2StorePenaltyFeruchemy = 1.5f;
    public const float Stage3StorePenaltyFeruchemy = 2.0f;

    public const float WithdrawalOnsetHours = 2f;
    public const float WithdrawalSeverityGainPerDay = 0.5f;
    public const float WithdrawalSeverityLossPerDay = 1.0f;

    public const float Stage1DecayPerDayFraction = 0.10f;

    private static readonly HashSet<string> DependencyMetals = [
        "Tin", "Pewter", "Brass", "Zinc", "Copper", "Bronze",
    ];

    private static readonly HashSet<string> GodMetals = [
        "Lerasium", "LerasiumAlloy", "Leratium", "LeratiumAlloy",
    ];

    private static readonly HashSet<string> NonAllomanticMetals = [
        "Harmonium", "Nickel", "Silver", "Trellium",
    ];

    public static bool CanBeSavant(MetalDef metal) {
        return !GodMetals.Contains(metal.defName) && !NonAllomanticMetals.Contains(metal.defName);
    }

    public static bool IsDependencyMetal(MetalDef metal) {
        return DependencyMetals.Contains(metal.defName);
    }

    public static bool IsIntegrationMetal(MetalDef metal) {
        return CanBeSavant(metal) && !IsDependencyMetal(metal);
    }

    public static int GetAllomanticStage(float ticks) {
        if (ticks >= AllomancyStage3Ticks) return 3;
        if (ticks >= AllomancyStage2Ticks) return 2;
        if (ticks >= AllomancyStage1Ticks) return 1;
        return 0;
    }

    public static int GetFeruchemicalStage(float ticks) {
        if (ticks >= FeruchemyStage3Ticks) return 3;
        if (ticks >= FeruchemyStage2Ticks) return 2;
        if (ticks >= FeruchemyStage1Ticks) return 1;
        return 0;
    }

    public static int GetSurgebindingStage(float ticks) {
        if (ticks >= SurgebindingStage3Ticks) return 3;
        if (ticks >= SurgebindingStage2Ticks) return 2;
        if (ticks >= SurgebindingStage1Ticks) return 1;
        return 0;
    }

    public static float GetAllomanticPowerMultiplier(int stage) {
        return stage switch {
            1 => Stage1PowerAllomancy,
            2 => Stage2PowerAllomancy,
            3 => Stage3PowerAllomancy,
            _ => 1f,
        };
    }

    public static float GetFeruchemicalPowerMultiplier(int stage) {
        return stage switch {
            1 => Stage1PowerFeruchemy,
            2 => Stage2PowerFeruchemy,
            3 => Stage3PowerFeruchemy,
            _ => 1f,
        };
    }

    public static float GetSurgebindingPowerMultiplier(int stage) {
        return stage switch {
            1 => Stage1PowerSurgebinding,
            2 => Stage2PowerSurgebinding,
            3 => Stage3PowerSurgebinding,
            _ => 1f,
        };
    }

    public static float GetSurgebindingCostMultiplier(int stage) {
        return stage switch {
            1 => Stage1CostSurgebinding,
            2 => Stage2CostSurgebinding,
            3 => Stage3CostSurgebinding,
            _ => 1f,
        };
    }

    public static float GetFeruchemyStorePenaltyMultiplier(int stage) {
        return stage switch {
            2 => Stage2StorePenaltyFeruchemy,
            3 => Stage3StorePenaltyFeruchemy,
            _ => 1f,
        };
    }

    public static float SeverityForStage(int stage) {
        return stage switch {
            1 => 0.2f,
            2 => 0.5f,
            3 => 0.9f,
            _ => 0f,
        };
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

    public static HediffDef? GetSurgeSavantHediffDef(string surgeName) {
        return DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Roshar_Hediff_SurgeSavant_" + surgeName);
    }

    public static HediffDef? GetSurgePermanentHediffDef(string surgeName) {
        return DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Roshar_Hediff_SurgeSavantPermanent_" + surgeName);
    }

    public static int GetAllomanticSavantStage(Pawn pawn, MetalDef metal) {
        RecordDef recordDef = System.Scadrial.RecordDefOf.GetTimeSpentBurningForMetal(metal);
        float ticks = pawn.records.GetValue(recordDef);
        return GetAllomanticStage(ticks);
    }

    public static int GetFeruchemicalSavantStage(Pawn pawn, MetalDef metal) {
        RecordDef storingRecord = System.Scadrial.RecordDefOf.GetTimeSpentStoringForMetal(metal);
        RecordDef tappingRecord = System.Scadrial.RecordDefOf.GetTimeSpentTappingForMetal(metal);
        float ticks = pawn.records.GetValue(storingRecord) + pawn.records.GetValue(tappingRecord);
        return GetFeruchemicalStage(ticks);
    }

    public static int GetSurgebindingSavantStage(Pawn pawn, System.Roshar.Def.SurgeDef surge) {
        RecordDef? recordDef = DefDatabase<RecordDef>.GetNamedSilentFail("Cosmere_Roshar_Record_TimeSpentUsing_" + surge.defName);
        if (recordDef == null) return 0;
        float ticks = pawn.records.GetValue(recordDef);
        return GetSurgebindingStage(ticks);
    }

    public static void SendSavantLetter(Pawn pawn, int stage, string systemKey, string powerName) {
        string titleKey = "CS_Savant_" + systemKey + "_Stage" + stage + "_Title";
        string messageKey = "CS_Savant_" + systemKey + "_Stage" + stage + "_Message";
        LetterDef letterDef = stage >= 2 ? LetterDefOf.NeutralEvent : LetterDefOf.PositiveEvent;

        Find.LetterStack.ReceiveLetter(
            titleKey.Translate(pawn.Named("PAWN"), powerName.Named("POWER")),
            messageKey.Translate(pawn.Named("PAWN"), powerName.Named("POWER")),
            letterDef,
            pawn
        );
    }
}
