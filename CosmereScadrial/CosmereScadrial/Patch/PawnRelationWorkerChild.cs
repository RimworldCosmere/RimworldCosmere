using HarmonyLib;
using RimWorld;
using Verse;
using GeneUtility = Cosmere.Scadrial.Util.GeneUtility;

namespace Cosmere.Scadrial.Patch;

[HarmonyPatch(typeof(PawnRelationWorker_Child), "CreateRelation")]
public static class PawnRelationWorkerChild {
    public static void Postfix(Pawn generated, Pawn other, PawnGenerationRequest request) {
        GeneUtility.TryAssignScadrialGenes(generated);
        GeneUtility.HandleSkaaPurityGenes(generated, other, request);
    }
}