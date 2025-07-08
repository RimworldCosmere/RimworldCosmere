using HarmonyLib;
using Verse;
using GeneUtility = CosmereScadrial.Util.GeneUtility;

namespace CosmereScadrial.Patch;

[HarmonyPatch]
public static class PawnGeneGeneration {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Verse.PawnGenerator), "GenerateGenes")]
    public static void PostfixGenerateGenes(Pawn? pawn, PawnGenerationRequest request) {
        if (pawn?.RaceProps.Humanlike != true) {
            return;
        }

        GeneUtility.TryAssignScadrialGenes(pawn);
        pawn.records.AddTo();
    }
}