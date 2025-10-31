using Cosmere;
﻿using Cosmere.System.Roshar.Comp.Thing;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Patch;

//Initial requirements, must have suffered crisis
[HarmonyPatch(typeof(MentalBreaker), nameof(MentalBreaker.MentalBreakerTickInterval))]
public static class Cosmere_Roshar_MentalBreakExperiences {
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
        } else if (__instance.BreakMajorIsImminent) {
            increment = 0.9f;
        } else if (__instance.BreakMinorIsImminent) {
            increment = 0.5f;
        }

        pawnTracker.bondChance += increment;
        pawnTracker.doCheckWhenThisIsZero = (pawnTracker.doCheckWhenThisIsZero + 1) % 100;
    }
}

////second requirements, must help people in need
[HarmonyPatch(typeof(TendUtility), nameof(TendUtility.DoTend))]
public static class Cosmere_Roshar_HelpSomeoneInNeed {
    private static void Postfix(Pawn? doctor, Pawn? patient) {
        if (doctor == null || patient == null || patient.NonHumanlikeOrWildMan() || doctor == patient) {
            return;
        }

        if (!doctor.TryGetComp(out PawnTracker pawnStats)) return;

        if (!pawnStats.patientList.Contains(patient)) {
            pawnStats.patientList.Add(patient);
        }
    }
}

//add class to check for eligible that both can use, and call from both!

[HarmonyPatch(typeof(Pawn_HealthTracker))]
public static class PatchPawnHealthTrackerHealthTick {
    private static readonly AccessTools.FieldRef<Pawn_HealthTracker, Pawn> pawnRef =
        AccessTools.FieldRefAccess<Pawn>(typeof(Pawn_HealthTracker), "pawn");

    [HarmonyPatch(nameof(Pawn_HealthTracker.HealthTickInterval))]
    private static void Postfix(Pawn_HealthTracker __instance, int delta) {
        if (!GenTicks.IsTickIntervalDelta(GenTicks.TickLongInterval, delta)) return;
        Pawn? pawn = pawnRef(__instance);
        if (pawn == null || pawn.NonHumanlikeOrWildMan()) {
            return;
        }

        if (!pawn.TryGetComp(out PawnTracker pawnTracker)) return;

        List<Pawn> patientsToRemove = [];
        foreach (Pawn patient in pawnTracker.patientList) {
            if (patient == null) {
                patientsToRemove.Add(patient);
                continue;
            }

            if (patient.health.Dead && !patient.IsPrisoner) {
                pawnTracker.OnPatientLost();
                patientsToRemove.Add(patient);
            } else if (NeedsNoTending(patient)) {
                pawnTracker.OnPatientSaved(patient.IsPrisonerOfColony);
                patientsToRemove.Add(patient);
            }
        }

        foreach (Pawn? patient in patientsToRemove) {
            pawnTracker.patientList.Remove(patient);
        }
    }

    private static bool NeedsNoTending(Pawn pawn) {
        return !pawn.health.HasHediffsNeedingTendByPlayer() && !HealthAIUtility.ShouldSeekMedicalRest(pawn);
    }
}