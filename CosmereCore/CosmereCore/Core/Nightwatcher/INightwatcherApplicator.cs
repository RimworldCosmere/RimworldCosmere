using Verse;

namespace Cosmere.Core.Nightwatcher;

public interface INightwatcherApplicator {
    void Apply(Pawn pawn, Verse.Def def, NightwatcherApplicationContext? context = null);
}
