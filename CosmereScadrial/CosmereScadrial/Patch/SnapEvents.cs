using CosmereScadrial.Util;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace CosmereScadrial.Patch;

[HarmonyPatch]
public static class SnapEvents {
    private static readonly AccessTools.FieldRef<MentalStateHandler, Pawn> mshPawn =
        AccessTools.FieldRefAccess<MentalStateHandler, Pawn>("pawn");

    private static readonly AccessTools.FieldRef<Pawn_HealthTracker, Pawn> phtPawn =
        AccessTools.FieldRefAccess<Pawn_HealthTracker, Pawn>("pawn");

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MentalStateHandler), nameof(MentalStateHandler.TryStartMentalState))]
    public static void SnapFromMentalBreak(MentalStateHandler __instance, bool __result) {
        if (__result) TrySnap(mshPawn(__instance));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.PostApplyDamage))]
    public static void SnapFromInjury(Pawn_HealthTracker __instance) {
        Pawn pawn = phtPawn(__instance);
        if (pawn.Dead || pawn.health.summaryHealth.SummaryHealthPercent > 0.2f) return;

        TrySnap(pawn);
    }

    private static void TrySnap(Pawn pawn) {
        if (!Rand.Chance(1f / 16f)) return;

        SnapUtility.TrySnap(pawn);
    }
}