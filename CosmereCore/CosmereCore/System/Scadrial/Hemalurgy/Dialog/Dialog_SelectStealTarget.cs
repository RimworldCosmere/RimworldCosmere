using System;
using Cosmere.Core.Def;
using Cosmere.System.Scadrial.Gene;
using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy.Dialog;

public static class StealTargetSelector {
    public static void ShowSelectionDialog(
        Pawn donor,
        HemalurgicStealType stealType,
        Action<GeneDef> onSelected,
        Action onCancel
    ) {
        List<GeneDef> candidates = GetStealCandidates(donor, stealType);
        if (candidates.Count == 0) {
            onCancel();
            return;
        }

        if (candidates.Count == 1) {
            onSelected(candidates[0]);
            return;
        }

        List<FloatMenuOption> options = [];
        for (int i = 0; i < candidates.Count; i++) {
            GeneDef gene = candidates[i];
            options.Add(new FloatMenuOption(gene.LabelCap, () => onSelected(gene)));
        }

        options.Add(new FloatMenuOption("Cancel", onCancel));

        Find.WindowStack.Add(new FloatMenu(options));
    }

    public static List<GeneDef> GetStealCandidates(Pawn donor, HemalurgicStealType stealType) {
        List<GeneDef> candidates = [];

        if (HemalurgicConstants.IsAllomanticSteal(stealType)) {
            string[] groupMetals = HemalurgicConstants.GetAllomanticGroupMetals(stealType);
            for (int i = 0; i < groupMetals.Length; i++) {
                MetalDef? metalDef = DefDatabase<MetalDef>.GetNamedSilentFail(groupMetals[i]);
                if (metalDef == null) continue;
                GeneDef? geneDef = metalDef.GetMistingGene();
                if (geneDef != null && donor.genes.HasActiveGene(geneDef)) {
                    candidates.Add(geneDef);
                }
            }
        } else if (HemalurgicConstants.IsFeruchemicSteal(stealType)) {
            string[] groupMetals = HemalurgicConstants.GetFeruchemicGroupMetals(stealType);
            for (int i = 0; i < groupMetals.Length; i++) {
                MetalDef? metalDef = DefDatabase<MetalDef>.GetNamedSilentFail(groupMetals[i]);
                if (metalDef == null) continue;
                GeneDef? geneDef = metalDef.GetFerringGene();
                if (geneDef != null && donor.genes.HasActiveGene(geneDef)) {
                    candidates.Add(geneDef);
                }
            }
        } else if (stealType == HemalurgicStealType.AnyPower) {
            List<Allomancer> allomantic = donor.genes.GetAllomanticGenes();
            for (int i = 0; i < allomantic.Count; i++) {
                candidates.Add(allomantic[i].def);
            }

            List<Feruchemist> feruchemic = donor.genes.GetFeruchemicGenes();
            for (int i = 0; i < feruchemic.Count; i++) {
                candidates.Add(feruchemic[i].def);
            }
        }

        return candidates;
    }
}