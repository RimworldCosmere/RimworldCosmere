using Cosmere.System.Roshar.Gene;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[HarmonyPatch(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.CapturedBy))]
public static class ArrestTrackingPatch {
    private static void Postfix(Pawn_GuestTracker __instance, Faction by, Pawn byPawn) {
        if (byPawn == null) return;
        if (by != Faction.OfPlayer) return;

        Surgebinder? surgebinder = byPawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        byPawn.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_ArrestsMade, 1);
    }
}
