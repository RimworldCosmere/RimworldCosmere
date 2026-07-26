using Cosmere.System.Roshar.Gene;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[HarmonyPatch(typeof(MentalState), nameof(MentalState.RecoverFromState))]
public static class MentalBreakTrackingPatch {
    private static void Postfix(MentalState __instance) {
        Pawn pawn = __instance.pawn;
        if (pawn == null || pawn.Dead) return;

        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        surgebinder.OnMentalBreakSurvived();
    }
}