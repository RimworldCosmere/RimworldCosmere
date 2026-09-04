using System;
using Cosmere.Core.Savant;
using Cosmere.System.Scadrial.Gene;
using Verse;

namespace Cosmere.System.Scadrial.Savant;

public class ScadrialSavantCandidateProvider : ISavantCandidateProvider {
    public void CollectCandidates(Pawn pawn, ICollection<Action> candidates) {
        if (pawn.genes == null) return;

        List<Verse.Gene> allGenes = pawn.genes.GenesListForReading;
        for (int i = 0; i < allGenes.Count; i++) {
            Verse.Gene gene = allGenes[i];
            if (gene is Allomancer allomancer && SavantUtility.CanBeSavant(allomancer.metal)) {
                Allomancer chosen = allomancer;
                candidates.Add(() => ApplyAllomanticSavant(pawn, chosen));
            } else if (gene is Feruchemist feruchemist && SavantUtility.CanBeSavant(feruchemist.metal)) {
                Feruchemist chosen = feruchemist;
                candidates.Add(() => ApplyFeruchemicalSavant(pawn, chosen));
            }
        }
    }

    private static void ApplyAllomanticSavant(Pawn pawn, Allomancer chosen) {
        SavantUtility.ApplySavantHediffs(
            pawn,
            ScadrialSavantUtility.GetAllomanticSavantHediffDef(chosen.metal),
            ScadrialSavantUtility.GetAllomanticPermanentHediffDef(chosen.metal)
        );
        Log.Info(
            $"ScadrialSavantCandidateProvider: forced {pawn.NameShortColored} to allomantic savant for {chosen.metal.defName}"
        );
    }

    private static void ApplyFeruchemicalSavant(Pawn pawn, Feruchemist chosen) {
        SavantUtility.ApplySavantHediffs(
            pawn,
            ScadrialSavantUtility.GetFeruchemicalSavantHediffDef(chosen.metal),
            ScadrialSavantUtility.GetFeruchemicalPermanentHediffDef(chosen.metal)
        );
        Log.Info(
            $"ScadrialSavantCandidateProvider: forced {pawn.NameShortColored} to feruchemical savant for {chosen.metal.defName}"
        );
    }
}
