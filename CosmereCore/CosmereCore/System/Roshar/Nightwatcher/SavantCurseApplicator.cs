using System.Collections.Generic;
using Cosmere.Core.Savant;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.Nightwatcher;

public class SavantCurseApplicator : ICurseApplicator {
    public void Apply(Pawn pawn, NightwatcherCurseDef def) {
        if (pawn.genes == null) return;

        List<Verse.Gene> allGenes = pawn.genes.GenesListForReading;

        List<Allomancer> allomancerGenes = [];
        List<Feruchemist> feruchemistGenes = [];
        List<Surgebinder> surgebinderGenes = [];

        for (int i = 0; i < allGenes.Count; i++) {
            if (allGenes[i] is Allomancer allomancer && SavantUtility.CanBeSavant(allomancer.metal)) {
                allomancerGenes.Add(allomancer);
            } else if (allGenes[i] is Feruchemist feruchemist && SavantUtility.CanBeSavant(feruchemist.metal)) {
                feruchemistGenes.Add(feruchemist);
            } else if (allGenes[i] is Surgebinder surgebinder) {
                surgebinderGenes.Add(surgebinder);
            }
        }

        int totalCandidates = allomancerGenes.Count + feruchemistGenes.Count + surgebinderGenes.Count;
        if (totalCandidates == 0) return;

        int pick = Rand.Range(0, totalCandidates);

        if (pick < allomancerGenes.Count) {
            Allomancer chosen = allomancerGenes[pick];
            ApplySavantHediffs(pawn,
                SavantUtility.GetAllomanticSavantHediffDef(chosen.metal),
                SavantUtility.GetAllomanticPermanentHediffDef(chosen.metal));
            Logger.Info($"SavantCurseApplicator: forced {pawn.NameShortColored} to allomantic savant for {chosen.metal.defName}");
            return;
        }
        pick -= allomancerGenes.Count;

        if (pick < feruchemistGenes.Count) {
            Feruchemist chosen = feruchemistGenes[pick];
            ApplySavantHediffs(pawn,
                SavantUtility.GetFeruchemicalSavantHediffDef(chosen.metal),
                SavantUtility.GetFeruchemicalPermanentHediffDef(chosen.metal));
            Logger.Info($"SavantCurseApplicator: forced {pawn.NameShortColored} to feruchemical savant for {chosen.metal.defName}");
            return;
        }
        pick -= feruchemistGenes.Count;

        Surgebinder chosenSurgebinder = surgebinderGenes[pick];
        List<SurgeDef> surges = chosenSurgebinder.radiantOrderDef.surges;
        if (surges.Count == 0) return;
        SurgeDef chosenSurge = surges[Rand.Range(0, surges.Count)];
        ApplySavantHediffs(pawn,
            SavantUtility.GetSurgeSavantHediffDef(chosenSurge.defName),
            SavantUtility.GetSurgePermanentHediffDef(chosenSurge.defName));
        Logger.Info($"SavantCurseApplicator: forced {pawn.NameShortColored} to surgebinding savant for {chosenSurge.defName}");
    }

    private static void ApplySavantHediffs(Pawn pawn, HediffDef? savantDef, HediffDef? permanentDef) {
        if (savantDef != null) {
            Verse.Hediff savant = pawn.health.hediffSet.GetFirstHediffOfDef(savantDef)
                ?? HediffMaker.MakeHediff(savantDef, pawn);
            savant.Severity = SavantUtility.SeverityForStage(3);
            if (!pawn.health.hediffSet.HasHediff(savantDef)) pawn.health.AddHediff(savant);
        }

        if (permanentDef != null && !pawn.health.hediffSet.HasHediff(permanentDef)) {
            pawn.health.AddHediff(HediffMaker.MakeHediff(permanentDef, pawn));
        }
    }
}
