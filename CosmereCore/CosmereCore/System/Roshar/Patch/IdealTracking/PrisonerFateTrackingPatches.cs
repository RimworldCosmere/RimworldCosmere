using Cosmere.System.Roshar.Gene;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[HarmonyPatch(typeof(GenGuest), nameof(GenGuest.PrisonerRelease))]
public static class PrisonerReleaseTrackingPatch {
    private static void Postfix(Pawn p) {
        if (p?.Map == null) return;

        List<Pawn> colonists = p.Map.mapPawns.FreeColonistsSpawned;
        for (int i = 0; i < colonists.Count; i++) {
            Pawn colonist = colonists[i];
            Surgebinder? surgebinder = colonist.genes?.GetFirstGeneOfType<Surgebinder>();
            if (surgebinder == null) continue;

            colonist.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_JudgmentsPassed, 1);
            colonist.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_PrisonersFreed, 2);
        }
    }
}

[HarmonyPatch(
    typeof(InteractionWorker_RecruitAttempt),
    nameof(InteractionWorker_RecruitAttempt.DoRecruit),
    typeof(Pawn),
    typeof(Pawn),
    typeof(bool)
)]
public static class RecruitTrackingPatch {
    private static void Postfix(Pawn recruiter, Pawn recruitee) {
        if (recruiter == null) return;

        Surgebinder? surgebinder = recruiter.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        recruiter.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_JudgmentsPassed, 1);
        recruiter.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_PrisonersFreed, 1);
    }
}

[HarmonyPatch(typeof(ExecutionUtility), nameof(ExecutionUtility.DoExecutionByCut))]
public static class ExecutionTrackingPatch {
    private static void Postfix(Pawn executioner, Pawn victim) {
        if (executioner == null) return;

        Surgebinder? surgebinder = executioner.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        executioner.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_JudgmentsPassed, 1);
    }
}
