using Cosmere.Core.Def;
using Cosmere.System.Roshar.Def;
using RimWorld;
using Verse;

namespace Cosmere.Core.Savant;

public readonly struct SavantProfile {
    public readonly int Stage1Ticks;
    public readonly int Stage2Ticks;
    public readonly int Stage3Ticks;
    public readonly float Stage1Power;
    public readonly float Stage2Power;
    public readonly float Stage3Power;

    public SavantProfile(
        int stage1Ticks,
        int stage2Ticks,
        int stage3Ticks,
        float stage1Power,
        float stage2Power,
        float stage3Power
    ) {
        Stage1Ticks = stage1Ticks;
        Stage2Ticks = stage2Ticks;
        Stage3Ticks = stage3Ticks;
        Stage1Power = stage1Power;
        Stage2Power = stage2Power;
        Stage3Power = stage3Power;
    }

    public int GetStage(float ticks) {
        if (ticks >= Stage3Ticks) return 3;
        if (ticks >= Stage2Ticks) return 2;
        if (ticks >= Stage1Ticks) return 1;
        return 0;
    }

    public float GetPowerMultiplier(int stage) {
        return stage switch {
            1 => Stage1Power,
            2 => Stage2Power,
            3 => Stage3Power,
            _ => 1f,
        };
    }
}

public static class SavantUtility {
    public const float Stage1CostSurgebinding = 0.90f;
    public const float Stage2CostSurgebinding = 0.80f;
    public const float Stage3CostSurgebinding = 0.70f;

    public const float Stage2StorePenaltyFeruchemy = 1.5f;
    public const float Stage3StorePenaltyFeruchemy = 2.0f;

    public const float WithdrawalOnsetHours = 2f;
    public const float WithdrawalSeverityGainPerDay = 0.5f;
    public const float WithdrawalSeverityLossPerDay = 1.0f;

    public const float Stage1DecayPerDayFraction = 0.10f;

    public static readonly SavantProfile AllomancyProfile = new SavantProfile(
        180000,
        600000,
        1800000,
        1.20f,
        1.40f,
        1.75f
    );

    public static readonly SavantProfile FeruchemyProfile = new SavantProfile(
        300000,
        900000,
        2400000,
        1.15f,
        1.35f,
        1.60f
    );

    public static readonly SavantProfile SurgebindingProfile = new SavantProfile(
        420000,
        1200000,
        3000000,
        1.15f,
        1.30f,
        1.50f
    );

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
        return AllomancyProfile.GetStage(ticks);
    }

    public static int GetFeruchemicalStage(float ticks) {
        return FeruchemyProfile.GetStage(ticks);
    }

    public static int GetSurgebindingStage(float ticks) {
        return SurgebindingProfile.GetStage(ticks);
    }

    public static float GetAllomanticPowerMultiplier(int stage) {
        return AllomancyProfile.GetPowerMultiplier(stage);
    }

    public static float GetFeruchemicalPowerMultiplier(int stage) {
        return FeruchemyProfile.GetPowerMultiplier(stage);
    }

    public static float GetSurgebindingPowerMultiplier(int stage) {
        return SurgebindingProfile.GetPowerMultiplier(stage);
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
        return DefDatabase<HediffDef>.GetNamedSilentFail(
            "Cosmere_Scadrial_Hediff_AllomanticWithdrawal_" + metal.defName
        );
    }

    public static HediffDef? GetAllomanticPermanentHediffDef(MetalDef metal) {
        return DefDatabase<HediffDef>.GetNamedSilentFail(
            "Cosmere_Scadrial_Hediff_AllomanticSavantPermanent_" + metal.defName
        );
    }

    public static HediffDef? GetFeruchemicalSavantHediffDef(MetalDef metal) {
        return DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_FeruchemicalSavant_" + metal.defName);
    }

    public static HediffDef? GetFeruchemicalPermanentHediffDef(MetalDef metal) {
        return DefDatabase<HediffDef>.GetNamedSilentFail(
            "Cosmere_Scadrial_Hediff_FeruchemicalSavantPermanent_" + metal.defName
        );
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

    public static int GetSurgebindingSavantStage(Pawn pawn, SurgeDef surge) {
        RecordDef? recordDef =
            DefDatabase<RecordDef>.GetNamedSilentFail("Cosmere_Roshar_Record_TimeSpentUsing_" + surge.defName);
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