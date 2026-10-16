using Concord;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[Patch(typeof(ResearchManager))]
public abstract class ResearchTrackingPatch {
    [Inject(At.Return, nameof(ResearchManager.FinishProject))]
    private void AfterFinishProject(ResearchProjectDef proj, Pawn researcher) {
        if (researcher == null) return;

        Surgebinder? surgebinder = researcher.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        researcher.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_ResearchCompleted, 1);
        surgebinder.NotifySkillGain();
    }
}
