using Cosmere.Core.Nightwatcher;
using Cosmere.System.Scadrial.Util;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.Nightwatcher;

public class MistbornApplicator : IBoonApplicator, INightwatcherEffectDescriber {
    public void Apply(Pawn pawn, Verse.Def def, NightwatcherApplicationContext? context = null) {
        GeneUtility.AddMistborn(pawn, false, true, "Nightwatcher's boon");
        Logger.Info($"MistbornApplicator: granted Mistborn to {pawn.NameShortColored}");
    }

    public string? DescribeEffects(NightwatcherApplicationContext? context = null) {
        return "Grants all Allomantic powers";
    }
}
