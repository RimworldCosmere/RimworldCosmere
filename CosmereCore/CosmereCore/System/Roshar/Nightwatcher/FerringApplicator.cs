using Cosmere.System.Roshar.Def;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Utility;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.Nightwatcher;

public class FerringApplicator : IBoonApplicator {
    public void Apply(Pawn pawn, NightwatcherBoonDef def, Dictionary<string, object>? context = null) {
        MetallicArtsMetalDef? metal = context?.TryGetValue("SelectedMetal", out object? value) == true
            ? value as MetallicArtsMetalDef
            : null;
        if (metal == null) return;
        GeneUtility.AddGene(
            pawn,
            Scadrial.GeneDefOf.GetFerringGeneForMetal(metal),
            false,
            true
        );
        Logger.Info($"FerringApplicator: granted Ferring ({metal.LabelCap}) to {pawn.NameShortColored}");
    }
}