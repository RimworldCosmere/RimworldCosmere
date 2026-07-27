using HarmonyLib;
using Verse;
using GeneUtility = Cosmere.System.Scadrial.Util.GeneUtility;

namespace Cosmere.System.Scadrial.Patch.Gene;

[HarmonyPatch]
public static class ScadrialGeneGenerationPatch {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Verse.PawnGenerator), "GenerateGenes")]
    public static void PostfixGenerateGenes(Pawn? pawn, PawnGenerationRequest request) {
        if (pawn?.RaceProps.Humanlike != true) {
            return;
        }

        GeneUtility.AssignScadrialGenes(pawn);
    }
}
