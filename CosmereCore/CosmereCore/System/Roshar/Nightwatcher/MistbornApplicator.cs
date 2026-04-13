using System.Collections.Generic;
using Cosmere.System.Roshar.Def;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.Nightwatcher;

public class MistbornApplicator : IBoonApplicator {
    public void Apply(Pawn pawn, NightwatcherBoonDef def, Dictionary<string, object>? context = null) {
        Scadrial.Utility.GeneUtility.AddMistborn(pawn, false, true, "Nightwatcher's boon");
        Logger.Info($"MistbornApplicator: granted Mistborn to {pawn.NameShortColored}");
    }
}
