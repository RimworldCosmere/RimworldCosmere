using Concord;
using Cosmere.System.Roshar.Comp.Thing;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.Radiant;

[Patch(typeof(TendUtility))]
public static class TendUtilityPatientTrackingPatch {
    [Inject(At.Return, nameof(TendUtility.DoTend))]
    private static void AfterDoTend(Pawn? doctor, Pawn? patient) {
        if (doctor == null || patient == null || patient.NonHumanlikeOrWildMan() || doctor == patient) {
            return;
        }

        if (!doctor.TryGetComp(out PawnTracker pawnStats)) return;

        if (!pawnStats.patientList.Contains(patient)) {
            pawnStats.patientList.Add(patient);
        }
    }
}
