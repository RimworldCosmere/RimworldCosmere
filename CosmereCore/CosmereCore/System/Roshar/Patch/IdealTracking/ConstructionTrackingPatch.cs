using Cosmere.System.Roshar.Gene;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[HarmonyPatch(typeof(Frame), nameof(Frame.CompleteConstruction))]
public static class ConstructionTrackingPatch {
    private static void Postfix(Pawn worker) {
        if (worker == null) return;

        Surgebinder? surgebinder = worker.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        worker.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_StructuresBuilt, 1);
    }
}