using HarmonyLib;
using Verse;
using GeneUtility = Cosmere.Scadrial.Utility.GeneUtility;

namespace Cosmere.Scadrial.Patch;

[HarmonyPatch]
public static class PawnGeneGeneration {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Verse.PawnGenerator), "GenerateGenes")]
    public static void PostfixGenerateGenes(Pawn? pawn, PawnGenerationRequest request) {
        if (pawn?.RaceProps.Humanlike != true) {
            return;
        }

        GeneUtility.TryAssignScadrialGenes(pawn);
    }
}