using Concord;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.World;

[Patch(typeof(Verse.PawnGenerator))]
public static class RosharGeneGenerationPatch {
    private static readonly string[] DahnGenes = [
        "Cosmere_Roshar_Gene_Dahn_High",
        "Cosmere_Roshar_Gene_Dahn_Mid",
        "Cosmere_Roshar_Gene_Dahn_Low",
    ];

    private static readonly float[] DahnWeights = [0.1f, 0.3f, 0.6f];

    private static readonly string[] NahnGenes = [
        "Cosmere_Roshar_Gene_Nahn_High",
        "Cosmere_Roshar_Gene_Nahn_Mid",
        "Cosmere_Roshar_Gene_Nahn_Low",
    ];

    private static readonly float[] NahnWeights = [0.2f, 0.5f, 0.3f];

    [Inject(At.Return, "GenerateGenes")]
    private static void AfterGenerateGenes(Pawn? pawn, PawnGenerationRequest request) {
        if (pawn?.RaceProps.Humanlike != true) return;
        if (pawn.genes == null) return;

        XenotypeDef? xenotype = pawn.genes.Xenotype;
        if (xenotype == null) return;

        if (xenotype == XenotypeDefOf.Cosmere_Roshar_Xenotype_Lighteyes) {
            AddRankGene(pawn, DahnGenes, DahnWeights);
        } else if (xenotype == XenotypeDefOf.Cosmere_Roshar_Xenotype_Darkeyes) {
            AddRankGene(pawn, NahnGenes, NahnWeights);
        }
    }

    private static void AddRankGene(Pawn pawn, string[] geneDefNames, float[] weights) {
        float roll = Rand.Value;
        float cumulative = 0f;
        string selected = geneDefNames[geneDefNames.Length - 1];

        for (int i = 0; i < weights.Length; i++) {
            cumulative += weights[i];
            if (roll < cumulative) {
                selected = geneDefNames[i];
                break;
            }
        }

        GeneDef? geneDef = DefDatabase<GeneDef>.GetNamedSilentFail(selected);
        if (geneDef == null) {
            Log.Warn($"PawnGeneGeneration: rank gene '{selected}' not found");
            return;
        }

        if (!pawn.genes.HasActiveGene(geneDef)) {
            pawn.genes.AddGene(geneDef, true);
            Log.Debug($"PawnGeneGeneration: assigned {selected} to {pawn.NameShortColored}");
        }
    }
}
