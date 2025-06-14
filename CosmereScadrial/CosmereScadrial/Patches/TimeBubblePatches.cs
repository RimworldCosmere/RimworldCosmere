using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereScadrial.Patches {
    [HarmonyPatch]
    public static class TimeBubblePatches {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Pawn_NeedsTracker), nameof(Pawn_NeedsTracker.NeedsTrackerTick))]
        public static bool Prefix(Pawn_NeedsTracker __instance) {
            const int baseInterval = 150;
            const int cadmiumMultiplier = 3;
            const int bendalloyDivisor = 3;

            var pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn?.health == null || pawn.Dead) return true;

            // If we are in a cadmium bubble, time slows down, needs should decay a third as fast
            if (pawn.health.hediffSet.HasHediff(HediffDef.Named("Cosmere_Hediff_TimeBubble_Cadmium"))) {
                if (!pawn.IsHashIntervalTick(baseInterval * cadmiumMultiplier)) {
                    return false;
                }
            } else if (pawn.health.hediffSet.HasHediff(HediffDef.Named("Cosmere_Hediff_TimeBubble_Bendalloy"))) {
                if (!pawn.IsHashIntervalTick(baseInterval / bendalloyDivisor)) {
                    return false;
                }
            } else {
                return true;
            }


            for (var index = 0; index < __instance.AllNeeds.Count; ++index) {
                __instance.AllNeeds[index].NeedInterval();
            }

            return false;
        }
    }
}