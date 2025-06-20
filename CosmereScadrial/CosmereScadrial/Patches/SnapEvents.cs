using CosmereScadrial.Utils;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace CosmereScadrial.Patches;

[HarmonyPatch]
public static class SnapEvents {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MentalStateHandler), nameof(MentalStateHandler.TryStartMentalState))]
    public static void SnapFromMentalBreak(Pawn pawn, bool result) {
        if (result) TrySnap(pawn);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.PostApplyDamage))]
    public static void SnapFromInjury(Pawn pawn) {
        if (pawn.Dead || pawn.health.summaryHealth.SummaryHealthPercent > 0.2f) return;

        TrySnap(pawn);
    }

    private static void TrySnap(Pawn pawn) {
        if (!Rand.Chance(1f / 16f)) return;

        SnapUtility.TrySnap(pawn);
    }
}