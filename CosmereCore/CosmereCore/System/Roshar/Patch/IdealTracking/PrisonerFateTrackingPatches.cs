using Concord;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[Patch(typeof(GenGuest))]
public static class PrisonerReleaseTrackingPatch {
    [Inject(At.Return, nameof(GenGuest.PrisonerRelease))]
    private static void AfterPrisonerRelease(Pawn p) {
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

[Patch]
public abstract class RecruitTrackingPatch : InteractionWorker_RecruitAttempt {
    [Inject(
        At.Return,
        nameof(DoRecruit),
        parameterTypes: [typeof(Pawn), typeof(Pawn), typeof(bool)]
    )]
    private static void AfterDoRecruit(Pawn recruiter, Pawn recruitee) {
        if (recruiter == null) return;

        Surgebinder? surgebinder = recruiter.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        recruiter.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_JudgmentsPassed, 1);
        recruiter.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_PrisonersFreed, 1);
    }
}

[Patch(typeof(ExecutionUtility))]
public static class ExecutionTrackingPatch {
    [Inject(At.Return, nameof(ExecutionUtility.DoExecutionByCut))]
    private static void AfterDoExecutionByCut(Pawn executioner, Pawn victim) {
        if (executioner == null) return;

        Surgebinder? surgebinder = executioner.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        executioner.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_JudgmentsPassed, 1);
    }
}
