using HarmonyLib;
using RimWorld;
using Verse;
using GeneUtility = Cosmere.System.Scadrial.Util.GeneUtility;

namespace Cosmere.System.Scadrial.Patch.Gene;

[HarmonyPatch(typeof(PawnRelationWorker_Child), "CreateRelation")]
public static class PawnRelationWorkerChildPatch {
    public static void Postfix(Pawn generated, Pawn other, PawnGenerationRequest request) {
        GeneUtility.AssignScadrialGenes(generated);
        GeneUtility.EnforceSkaaPurityInheritance(generated, other, request);
    }
}
