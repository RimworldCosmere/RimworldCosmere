using Cosmere.Core.Nightwatcher;
using Cosmere.System.Scadrial.Util;
using Verse;

namespace Cosmere.System.Scadrial.Nightwatcher;

public class FullFeruchemistApplicator : IBoonApplicator, INightwatcherEffectDescriber {
    public void Apply(Pawn pawn, Verse.Def def, NightwatcherApplicationContext? context = null) {
        GeneUtility.AddFullFeruchemist(pawn, false, true, "Nightwatcher's boon");
        Log.Info($"FullFeruchemistApplicator: granted Full Feruchemist to {pawn.NameShortColored}");
    }

    public string? DescribeEffects(NightwatcherApplicationContext? context = null) {
        return "Grants all Feruchemical powers";
    }
}
