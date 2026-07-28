using Concord;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[Patch(typeof(TendUtility))]
public static class TendTrackingPatch {
    [Inject(At.Return, nameof(TendUtility.DoTend))]
    private static void AfterDoTend(Pawn doctor, Pawn patient, Medicine medicine) {
        if (doctor == null || patient == null) return;
        if (doctor == patient) return;

        Surgebinder? surgebinder = doctor.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        if (patient.IsColonist) return;

        doctor.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_ForgottenTended, 1);
    }
}
