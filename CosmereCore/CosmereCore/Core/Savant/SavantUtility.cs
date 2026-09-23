using Cosmere.Core.Def;
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

    public static float GetAllomanticPowerMultiplier(int stage) {
        return AllomancyProfile.GetPowerMultiplier(stage);
    }

    public static float GetFeruchemicalPowerMultiplier(int stage) {
        return FeruchemyProfile.GetPowerMultiplier(stage);
    }

    public static float SeverityForStage(int stage) {
        return stage switch {
            1 => 0.2f,
            2 => 0.5f,
            3 => 0.9f,
            _ => 0f,
        };
    }

    public static void ApplySavantHediffs(Pawn pawn, HediffDef? savantDef, HediffDef? permanentDef) {
        if (savantDef != null) {
            Verse.Hediff savant = pawn.health.hediffSet.GetFirstHediffOfDef(savantDef) ??
                                  HediffMaker.MakeHediff(savantDef, pawn);
            savant.Severity = SeverityForStage(3);
            if (!pawn.health.hediffSet.HasHediff(savantDef)) pawn.health.AddHediff(savant);
        }

        if (permanentDef != null && !pawn.health.hediffSet.HasHediff(permanentDef)) {
            pawn.health.AddHediff(HediffMaker.MakeHediff(permanentDef, pawn));
        }
    }

    public static void UpdateSavantHediffState(
        Pawn pawn,
        int previousStage,
        int newStage,
        HediffDef? savantHediffDef,
        HediffDef? permanentHediffDef
    ) {
        if (newStage > 0 && savantHediffDef != null) {
            Verse.Hediff? existing = pawn.health.hediffSet.GetFirstHediffOfDef(savantHediffDef);
            if (existing is null) {
                Verse.Hediff hediff = HediffMaker.MakeHediff(savantHediffDef, pawn);
                hediff.Severity = SeverityForStage(newStage);
                pawn.health.AddHediff(hediff);
            } else {
                existing.Severity = SeverityForStage(newStage);
            }
        } else if (newStage == 0 && savantHediffDef != null) {
            Verse.Hediff? existing = pawn.health.hediffSet.GetFirstHediffOfDef(savantHediffDef);
            if (existing is not null) pawn.health.RemoveHediff(existing);
        }

        if (newStage >= 3 && previousStage < 3 && permanentHediffDef != null) {
            if (!pawn.health.hediffSet.HasHediff(permanentHediffDef)) {
                pawn.health.AddHediff(HediffMaker.MakeHediff(permanentHediffDef, pawn));
            }
        }
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
