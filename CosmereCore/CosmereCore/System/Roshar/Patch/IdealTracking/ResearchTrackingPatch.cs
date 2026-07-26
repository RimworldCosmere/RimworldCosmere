using Cosmere.System.Roshar.Gene;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.FinishProject))]
public static class ResearchTrackingPatch {
    private static void Postfix(ResearchProjectDef proj, Pawn researcher) {
        if (researcher == null) return;

        Surgebinder? surgebinder = researcher.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        researcher.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_ResearchCompleted, 1);
        surgebinder.NotifySkillGain();
    }
}