using Cosmere.System.Roshar.Gene;
using HarmonyLib;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[HarmonyPatch(typeof(Pawn_HealthTracker), "MakeUndowned")]
public static class DownedRecoveryTrackingPatch {
    private static readonly AccessTools.FieldRef<Pawn_HealthTracker, Pawn> PawnRef =
        AccessTools.FieldRefAccess<Pawn>(typeof(Pawn_HealthTracker), "pawn");

    private static void Postfix(Pawn_HealthTracker __instance) {
        Pawn pawn = PawnRef(__instance);
        if (pawn == null) return;

        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        pawn.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_ChallengesSurvived, 1);
    }
}
