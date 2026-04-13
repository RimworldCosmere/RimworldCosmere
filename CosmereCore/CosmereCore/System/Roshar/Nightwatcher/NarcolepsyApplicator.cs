using Cosmere.System.Roshar.Comp.Game;
using Cosmere.System.Roshar.Def;
using Verse;

namespace Cosmere.System.Roshar.Nightwatcher;

public class NarcolepsyApplicator : ICurseApplicator {
    public void Apply(Pawn pawn, NightwatcherCurseDef def) {
        NarcolepsyTracker.Instance?.Register(pawn);
    }
}
