using CosmereScadrial.Utils;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereScadrial.Patches {
    [HarmonyPatch(typeof(PawnRelationWorker_Child), "CreateRelation")]
    public static class PawnRelationWorkerChild {
        public static void Postfix(Pawn generated, Pawn other, PawnGenerationRequest request) {
            GeneInheritanceUtility.TryAssignScadrialGenes(generated);
            GeneInheritanceUtility.HandleSkaaPurityGenes(generated, other, request);
        }
    }
}