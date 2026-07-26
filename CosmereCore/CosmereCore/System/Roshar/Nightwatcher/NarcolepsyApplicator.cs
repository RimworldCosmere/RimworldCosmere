using Cosmere.Core.Nightwatcher;
using Cosmere.System.Roshar.Comp.Game;
using Verse;

namespace Cosmere.System.Roshar.Nightwatcher;

public class NarcolepsyApplicator : ICurseApplicator, INightwatcherEffectDescriber {
    public string? DescribeEffects(NightwatcherApplicationContext? context = null) {
        return "Random narcoleptic episodes";
    }

    public void Apply(Pawn pawn, Verse.Def def, NightwatcherApplicationContext? context = null) {
        NarcolepsyTracker.Instance?.Register(pawn);
    }
}