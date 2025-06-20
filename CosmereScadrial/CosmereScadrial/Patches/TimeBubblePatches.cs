using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereScadrial.Patches;

[HarmonyPatch]
public static class TimeBubblePatches {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Pawn_NeedsTracker), nameof(Pawn_NeedsTracker.NeedsTrackerTickInterval))]
    public static bool Prefix(Pawn_NeedsTracker instance, int delta) {
        const int BaseInterval = 150;
        const int CadmiumMultiplier = 3;
        const int BendalloyDivisor = 3;

        Pawn? pawn = Traverse.Create(instance).Field("pawn").GetValue<Pawn>();
        if (pawn?.health == null || pawn.Dead) return true;

        // If we are in a cadmium bubble, time slows down, needs should decay a third as fast
        if (pawn.health.hediffSet.HasHediff(HediffDef.Named("Cosmere_Hediff_TimeBubble_Cadmium"))) {
            if (!pawn.IsHashIntervalTick(BaseInterval * CadmiumMultiplier, delta)) {
                return false;
            }
        } else if (pawn.health.hediffSet.HasHediff(HediffDef.Named("Cosmere_Hediff_TimeBubble_Bendalloy"))) {
            if (!pawn.IsHashIntervalTick(BaseInterval / BendalloyDivisor, delta)) {
                return false;
            }
        } else {
            return true;
        }


        for (int index = 0; index < instance.AllNeeds.Count; ++index) {
            instance.AllNeeds[index].NeedInterval();
        }

        return false;
    }
}