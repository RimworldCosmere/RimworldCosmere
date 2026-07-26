using Cosmere.System.Roshar.Gene;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[HarmonyPatch(typeof(TendUtility), nameof(TendUtility.DoTend))]
public static class TendTrackingPatch {
    private static void Postfix(Pawn doctor, Pawn patient, Medicine medicine) {
        if (doctor == null || patient == null) return;
        if (doctor == patient) return;

        Surgebinder? surgebinder = doctor.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        if (patient.IsColonist) return;

        doctor.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_ForgottenTended, 1);
    }
}