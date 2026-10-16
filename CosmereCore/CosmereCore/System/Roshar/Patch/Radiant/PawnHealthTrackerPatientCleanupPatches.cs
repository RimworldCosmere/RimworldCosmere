using Concord;
using Cosmere.System.Roshar.Comp.Thing;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.Radiant;

[Patch]
public abstract class PawnHealthTrackerPatientCleanupPatch : Pawn_HealthTracker {
    [InjectField("pawn")]
    private readonly Pawn trackedPawn = null!;

    protected PawnHealthTrackerPatientCleanupPatch(Pawn pawn) : base(pawn) { }

    [Inject(At.Return, nameof(HealthTickInterval))]
    private void AfterHealthTickInterval(int delta) {
        if (!GenTicks.IsTickIntervalDelta(GenTicks.TickLongInterval, delta)) return;
        Pawn? pawn = trackedPawn;
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
