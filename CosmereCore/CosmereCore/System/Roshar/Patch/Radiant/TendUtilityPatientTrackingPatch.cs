using Cosmere.System.Roshar.Comp.Thing;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.Radiant;

[HarmonyPatch(typeof(TendUtility), nameof(TendUtility.DoTend))]
public static class TendUtilityPatientTrackingPatch {
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
