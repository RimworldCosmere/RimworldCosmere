using Cosmere.System.Roshar.Comp.Thing;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Patch.Radiant;

[HarmonyPatch(typeof(Pawn_HealthTracker))]
public static class PawnHealthTrackerPatientCleanupPatch {
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
                patientsToRemove.Add(patient!);
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
