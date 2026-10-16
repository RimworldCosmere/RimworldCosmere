using Cosmere.Core.Savant;
using Cosmere.System.Roshar.Def;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Savant;

public static class SurgebindingSavantUtility {
    public const float Stage1Cost = 0.90f;
    public const float Stage2Cost = 0.80f;
    public const float Stage3Cost = 0.70f;

    public static readonly SavantProfile Profile = new SavantProfile(
        420000,
        1200000,
        3000000,
        1.15f,
        1.30f,
        1.50f
    );

    public static int GetStage(float ticks) {
        return Profile.GetStage(ticks);
    }

    public static float GetPowerMultiplier(int stage) {
        return Profile.GetPowerMultiplier(stage);
    }

    public static float GetCostMultiplier(int stage) {
        return stage switch {
            1 => Stage1Cost,
            2 => Stage2Cost,
            3 => Stage3Cost,
            _ => 1f,
        };
    }

    public static HediffDef? GetSavantHediffDef(string surgeName) {
        return DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Roshar_Hediff_SurgeSavant_" + surgeName);
    }

    public static HediffDef? GetPermanentHediffDef(string surgeName) {
        return DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Roshar_Hediff_SurgeSavantPermanent_" + surgeName);
    }

    public static int GetSavantStage(Pawn pawn, SurgeDef surge) {
        RecordDef? recordDef =
            DefDatabase<RecordDef>.GetNamedSilentFail("Cosmere_Roshar_Record_TimeSpentUsing_" + surge.defName);
        if (recordDef == null) return 0;
        float ticks = pawn.records.GetValue(recordDef);
        return GetStage(ticks);
    }
}
