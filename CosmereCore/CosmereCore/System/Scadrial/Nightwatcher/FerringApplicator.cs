using Cosmere.Core.Nightwatcher;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Util;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.Nightwatcher;

public class FerringApplicator : IBoonApplicator, INightwatcherChoiceProvider, INightwatcherEffectDescriber {
    public void Apply(Pawn pawn, Verse.Def def, NightwatcherApplicationContext? context = null) {
        if (context?.SelectedDefName == null) return;
        MetallicArtsMetalDef? metal = DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(context.SelectedDefName);
        if (metal == null) return;
        GeneUtility.AddGene(
            pawn,
            Scadrial.GeneDefOf.GetFerringGeneForMetal(metal),
            false,
            true
        );
        Logger.Info($"FerringApplicator: granted Ferring ({metal.LabelCap}) to {pawn.NameShortColored}");
    }

    public IEnumerable<NightwatcherChoice> GetChoices(Verse.Def def) {
        List<MetallicArtsMetalDef> all = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading;
        for (int i = 0; i < all.Count; i++) {
            MetallicArtsMetalDef metal = all[i];
            if (metal.feruchemy == null) continue;
            string userName = metal.feruchemy.userName ?? metal.LabelCap;
            yield return new NightwatcherChoice(
                metal.defName,
                $"{userName} ({metal.LabelCap})"
            );
        }
    }

    public string? DescribeEffects(NightwatcherApplicationContext? context = null) {
        if (context?.SelectedDefName == null) return "Grants one Feruchemical power (choose metal)";
        MetallicArtsMetalDef? metal = DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(context.SelectedDefName);
        if (metal?.feruchemy == null) return "Grants one Feruchemical power (choose metal)";
        string userName = metal.feruchemy.userName ?? metal.LabelCap;
        return $"Grants Feruchemy: {userName} ({metal.LabelCap})";
    }
}
