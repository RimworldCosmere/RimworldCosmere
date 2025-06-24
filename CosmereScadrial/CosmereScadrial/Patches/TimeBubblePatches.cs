using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Patches;

[HarmonyPatch]
public static class TimeBubblePatches {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Pawn_NeedsTracker), nameof(Pawn_NeedsTracker.NeedsTrackerTickInterval))]
    public static bool Prefix(Pawn_NeedsTracker __instance, int delta) {
        const int BaseInterval = 150;
        const int CadmiumMultiplier = 3;
        const int BendalloyDivisor = 3;

        Pawn? pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
        if (pawn?.health == null || pawn.Dead) return true;

        // If we are in a cadmium bubble, time slows down, needs should decay a third as fast
        if (pawn.health.hediffSet.HasHediff(HediffDef.Named("Cosmere_Hediff_Time_Bubble_Cadmium"))) {
            if (!pawn.IsHashIntervalTick(BaseInterval * CadmiumMultiplier, delta)) {
                return false;
            }
        } else if (pawn.health.hediffSet.HasHediff(HediffDef.Named("Cosmere_Hediff_Time_Bubble_Bendalloy"))) {
            if (!pawn.IsHashIntervalTick(Mathf.RoundToInt((float)BaseInterval / BendalloyDivisor), delta)) {
                return false;
            }
        } else {
            return true;
        }


        for (int index = 0; index < __instance.AllNeeds.Count; ++index) {
            __instance.AllNeeds[index].NeedInterval();
        }

        return false;
    }
}