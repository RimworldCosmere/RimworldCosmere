using CosmereCore.Genes;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereScadrial.Patches
{
    [HarmonyPatch(typeof(PawnRelationWorker_Child), "CreateRelation")]
    public static class Patch_PawnRelationWorker_Child
    {
        public static void Postfix(Pawn generated, Pawn other, PawnGenerationRequest request)
        {
            GeneInheritanceUtility.TryAssignScadrialGenes(generated);
            GeneInheritanceUtility.HandleSkaaPurityGenes(generated, other, request);
        }
    }
}