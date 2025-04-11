using CosmereCore.Genes;
using HarmonyLib;
using Verse;

namespace CosmereCore.Patches
{
    [HarmonyPatch(typeof(PawnGenerator), "GenerateGenes")]
    public static class Patch_PawnGeneGeneration
    {
        public static void Postfix(Pawn pawn, PawnGenerationRequest request)
        {
            if (pawn == null || !pawn.RaceProps.Humanlike)
                return;

            GeneInheritanceUtility.TryAssignScadrialGenes(pawn);
        }
    }
}