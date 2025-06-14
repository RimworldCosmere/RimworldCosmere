using CosmereScadrial.Utils;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace CosmereScadrial.Patches {
    [HarmonyPatch]
    public static class SnapEvents {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MentalStateHandler), nameof(MentalStateHandler.TryStartMentalState))]
        public static void SnapFromMentalBreak(Pawn ___pawn, bool __result) {
            if (__result) TrySnap(___pawn);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.PostApplyDamage))]
        public static void SnapFromInjury(Pawn ___pawn) {
            if (___pawn.Dead || ___pawn.health.summaryHealth.SummaryHealthPercent > 0.2f) return;

            TrySnap(___pawn);
        }

        private static void TrySnap(Pawn pawn) {
            if (!Rand.Chance(1f / 16f)) return;

            SnapUtility.TrySnap(pawn);
        }
    }
}