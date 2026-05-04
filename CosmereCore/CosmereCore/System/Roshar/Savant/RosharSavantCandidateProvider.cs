using System;
using Cosmere.Core.Savant;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.Savant;

public class RosharSavantCandidateProvider : ISavantCandidateProvider {
    public void CollectCandidates(Pawn pawn, ICollection<Action> candidates) {
        if (pawn.genes == null) return;

        List<Verse.Gene> allGenes = pawn.genes.GenesListForReading;
        for (int i = 0; i < allGenes.Count; i++) {
            if (allGenes[i] is not Surgebinder surgebinder) continue;
            Surgebinder chosen = surgebinder;
            candidates.Add(() => ApplySurgebindingSavant(pawn, chosen));
        }
    }

    private static void ApplySurgebindingSavant(Pawn pawn, Surgebinder surgebinder) {
        List<SurgeDef> surges = surgebinder.radiantOrderDef.surges;
        if (surges.Count == 0) return;
        SurgeDef chosenSurge = surges[Rand.Range(0, surges.Count)];
        SavantUtility.ApplySavantHediffs(
            pawn,
            SurgebindingSavantUtility.GetSavantHediffDef(chosenSurge.defName),
            SurgebindingSavantUtility.GetPermanentHediffDef(chosenSurge.defName)
        );
        Logger.Info(
            $"RosharSavantCandidateProvider: forced {pawn.NameShortColored} to surgebinding savant for {chosenSurge.defName}"
        );
    }

}
