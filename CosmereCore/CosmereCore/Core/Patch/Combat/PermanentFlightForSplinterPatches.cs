using Cosmere.Core.Thing;

using HarmonyLib;
using RimWorld;
using Verse;
namespace Cosmere.Core.Patch;

[HarmonyPatch(typeof(Pawn_FlightTracker))]
public static class PermanentFlightForSplinterPatch {
    [HarmonyPatch(nameof(Pawn_FlightTracker.FlightTick))]
    [HarmonyPostfix]
    public static void PostfixFlightTick(Pawn ___pawn, ref int ___flyingTicks, ref int ___flightState) {
        if (___pawn is Splinter) {
            ___flyingTicks = 0;
            ___flightState = 1;
        }
    }

    [HarmonyPatch(nameof(Pawn_FlightTracker.ForceLand))]
    [HarmonyPrefix]
    public static bool PrefixForceLand(Pawn ___pawn) {
        if (___pawn is Splinter) return false;

        return true;
    }
}