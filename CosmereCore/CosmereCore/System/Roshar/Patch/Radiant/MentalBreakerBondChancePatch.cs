using Cosmere.System.Roshar.Comp.Thing;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Patch.Radiant;

[HarmonyPatch(typeof(MentalBreaker), nameof(MentalBreaker.MentalBreakerTickInterval))]
public static class MentalBreakerBondChancePatch {
    private static readonly AccessTools.FieldRef<MentalBreaker, Pawn> pawnRef =
        AccessTools.FieldRefAccess<Pawn>(typeof(MentalBreaker), "pawn");

    private static void Postfix(MentalBreaker __instance, int delta) {
        if (!GenTicks.IsTickIntervalDelta(GenTicks.TicksPerRealSecond, delta)) return;
        Pawn? pawn = pawnRef(__instance);
        if (pawn.NonHumanlikeOrWildMan() || !pawn.IsColonist) return;
        PawnTracker pawnTracker = pawn.GetComp<PawnTracker>();
        if (pawnTracker == null || pawn.records.GetAsInt(RecordDefOf.Cosmere_Roshar_Record_BondsFormed) > 0) return;

        float increment = 0f;
        if (__instance.BreakExtremeIsImminent) {
            increment = 2.5f;
        }
        else if (__instance.BreakMajorIsImminent) {
            increment = 0.9f;
        }
        else if (__instance.BreakMinorIsImminent) {
            increment = 0.5f;
        }

        pawnTracker.bondChance += increment;
        pawnTracker.doCheckWhenThisIsZero = (pawnTracker.doCheckWhenThisIsZero + 1) % 100;
    }
}
