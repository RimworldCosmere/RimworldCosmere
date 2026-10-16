using Cosmere.Core.Nightwatcher;
using Cosmere.Core.Savant;
using Verse;

namespace Cosmere.System.Roshar.Nightwatcher;

public class SavantCurseApplicator : ICurseApplicator {
    public void Apply(Pawn pawn, Verse.Def def, NightwatcherApplicationContext? context = null) {
        if (pawn.genes == null) return;
        SavantCandidateRegistry.ApplyRandomSavant(pawn);
    }
}
