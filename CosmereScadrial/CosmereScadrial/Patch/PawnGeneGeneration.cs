using CosmereScadrial.Util;
using HarmonyLib;
using Verse;

namespace CosmereScadrial.Patch;

[HarmonyPatch(typeof(Verse.PawnGenerator), "GenerateGenes")]
public static class PawnGeneGeneration {
    public static void Postfix(Pawn pawn, PawnGenerationRequest request) {
        if (pawn == null || !pawn.RaceProps.Humanlike) {
            return;
        }

        GeneUtility.TryAssignScadrialGenes(pawn);
    }
}