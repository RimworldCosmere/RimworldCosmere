using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Feruchemy;

[HarmonyPatch]
public static class TimeBubblePatch {
    private static readonly AccessTools.FieldRef<Pawn_NeedsTracker, Pawn> PawnField =
        AccessTools.FieldRefAccess<Pawn_NeedsTracker, Pawn>("pawn");

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Pawn_NeedsTracker), nameof(Pawn_NeedsTracker.NeedsTrackerTickInterval))]
    public static bool Prefix(Pawn_NeedsTracker __instance, int delta) {
        const int BaseInterval = 150;
        const int CadmiumMultiplier = 3;
        const int BendalloyDivisor = 3;

        Pawn? pawn = PawnField(__instance);
        if (pawn?.health == null || pawn.Dead) return true;


        // If we are in a cadmium bubble, time slows down, needs should decay a third as fast
        if (pawn.health.hediffSet.HasHediff(HediffDefOf.Cosmere_Scadrial_Hediff_TimeBubbleCadmium)) {
            if (!pawn.IsHashIntervalTick(BaseInterval * CadmiumMultiplier, delta)) {
                return false;
            }
        }
        else if (pawn.health.hediffSet.HasHediff(HediffDefOf.Cosmere_Scadrial_Hediff_TimeBubbleBendalloy)) {
            if (!pawn.IsHashIntervalTick(Mathf.RoundToInt((float)BaseInterval / BendalloyDivisor), delta)) {
                return false;
            }
        }
        else {
            return true;
        }


        for (int index = 0; index < __instance.AllNeeds.Count; ++index) {
            __instance.AllNeeds[index].NeedInterval();
        }

        return false;
    }
}