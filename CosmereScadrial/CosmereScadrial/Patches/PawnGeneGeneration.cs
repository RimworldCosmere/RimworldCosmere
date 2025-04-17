using CosmereScadrial.Utils;
using HarmonyLib;
using Verse;

namespace CosmereScadrial.Patches {
    [HarmonyPatch(typeof(Verse.PawnGenerator), "GenerateGenes")]
    public static class PawnGeneGeneration {
        public static void Postfix(Pawn pawn, PawnGenerationRequest request) {
            if (pawn == null || !pawn.RaceProps.Humanlike) {
                return;
            }

            GeneInheritanceUtility.TryAssignScadrialGenes(pawn);
        }
    }
}