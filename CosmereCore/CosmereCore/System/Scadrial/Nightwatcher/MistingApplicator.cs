using Cosmere.Core.Nightwatcher;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Util;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.Nightwatcher;

public class MistingApplicator : IBoonApplicator, INightwatcherChoiceProvider, INightwatcherEffectDescriber {
    public void Apply(Pawn pawn, Verse.Def def, NightwatcherApplicationContext? context = null) {
        if (context?.SelectedDefName == null) return;
        MetallicArtsMetalDef? metal = DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(context.SelectedDefName);
        if (metal == null) return;
        GeneUtility.AddGene(
            pawn,
            Scadrial.GeneDefOf.GetMistingGeneForMetal(metal),
            false,
            true
        );
        Logger.Info($"MistingApplicator: granted Misting ({metal.LabelCap}) to {pawn.NameShortColored}");
    }

    public IEnumerable<NightwatcherChoice> GetChoices(Verse.Def def) {
        List<MetallicArtsMetalDef> all = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading;
        for (int i = 0; i < all.Count; i++) {
            MetallicArtsMetalDef metal = all[i];
            if (metal.allomancy?.userName == null) continue;
            yield return new NightwatcherChoice(
                metal.defName,
                $"{metal.allomancy.userName} ({metal.LabelCap})"
            );
        }
    }

    public string? DescribeEffects(NightwatcherApplicationContext? context = null) {
        if (context?.SelectedDefName == null) return "Grants one Allomantic power (choose metal)";
        MetallicArtsMetalDef? metal = DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(context.SelectedDefName);
        if (metal?.allomancy?.userName == null) return "Grants one Allomantic power (choose metal)";
        return $"Grants Allomancy: {metal.allomancy.userName} ({metal.LabelCap})";
    }
}
