using System;
using Cosmere.Core.Def;
using Cosmere.System.Scadrial.Gene;
using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy;

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

        options.Add(new FloatMenuOption("CancelButton".Translate(), onCancel));

        Find.WindowStack.Add(new FloatMenu(options));
    }

    public static List<GeneDef> GetStealCandidates(Pawn donor, HemalurgicStealType stealType) {
        List<GeneDef> candidates = [];

        if (HemalurgicConstants.IsAllomanticSteal(stealType)) {
            AddGroupGenes(candidates, donor, HemalurgicConstants.GetAllomanticGroupMetals(stealType), true);
        } else if (HemalurgicConstants.IsFeruchemicSteal(stealType)) {
            AddGroupGenes(candidates, donor, HemalurgicConstants.GetFeruchemicGroupMetals(stealType), false);
        } else if (stealType == HemalurgicStealType.AnyPower) {
            AddEveryPower(candidates, donor);
        }

        return candidates;
    }

    /// <summary>The genes of one metal group the donor actually has, allomantic or feruchemic.</summary>
    private static void AddGroupGenes(List<GeneDef> candidates, Pawn donor, string[] groupMetals, bool allomantic) {
        for (int i = 0; i < groupMetals.Length; i++) {
            MetalDef? metalDef = DefDatabase<MetalDef>.GetNamedSilentFail(groupMetals[i]);
            if (metalDef == null) continue;

            GeneDef? geneDef = allomantic ? metalDef.GetMistingGene() : metalDef.GetFerringGene();
            if (geneDef != null && donor.genes.HasActiveGene(geneDef)) {
                candidates.Add(geneDef);
            }
        }
    }

    private static void AddEveryPower(List<GeneDef> candidates, Pawn donor) {
        List<Allomancer> allomantic = donor.genes.GetAllomanticGenes();
        for (int i = 0; i < allomantic.Count; i++) {
            candidates.Add(allomantic[i].def);
        }

        List<Feruchemist> feruchemic = donor.genes.GetFeruchemicGenes();
        for (int i = 0; i < feruchemic.Count; i++) {
            candidates.Add(feruchemic[i].def);
        }
    }
}
