using Cosmere.System.Roshar.Gene;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[HarmonyPatch(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.Notify_RescuedBy))]
public static class RescueTrackingPatch {
    private static readonly AccessTools.FieldRef<Pawn_RelationsTracker, Pawn> PawnRef =
        AccessTools.FieldRefAccess<Pawn>(typeof(Pawn_RelationsTracker), "pawn");

    private static void Postfix(Pawn_RelationsTracker __instance, Pawn rescuer) {
        Surgebinder? surgebinder = rescuer.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        rescuer.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_PawnsRescued, 1);

        Pawn rescued = PawnRef(__instance);
        if (rescued?.Faction == null) return;
        if (!rescued.Faction.HostileTo(rescuer.Faction)) return;

        rescuer.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_HostilePawnsRescued, 1);
    }
}