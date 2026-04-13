using Cosmere.System.Roshar.Def;
using Cosmere.System.Scadrial.Def;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.Nightwatcher;

public class MistingApplicator : IBoonApplicator {
    public void Apply(Pawn pawn, NightwatcherBoonDef def, Dictionary<string, object>? context = null) {
        MetallicArtsMetalDef? metal = context?.TryGetValue("SelectedMetal", out object? value) == true
            ? value as MetallicArtsMetalDef
            : null;
        if (metal == null) return;
        Scadrial.Utility.GeneUtility.AddGene(
            pawn,
            Scadrial.GeneDefOf.GetMistingGeneForMetal(metal),
            false,
            true
        );
        Logger.Info($"MistingApplicator: granted Misting ({metal.LabelCap}) to {pawn.NameShortColored}");
    }
}
