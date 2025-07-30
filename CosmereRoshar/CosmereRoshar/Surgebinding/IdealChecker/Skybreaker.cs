using Cosmere.Roshar.Def;
using Cosmere.Roshar.Gene;
using Verse;

namespace Cosmere.Roshar.Surgebinding.IdealChecker;

public class Skybreaker(RadiantOrderDef def) : AbstractIdealChecker(def) {
    public override bool IsSatisfied(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        return true;
    }

    public override bool Satisfy(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        throw new System.NotImplementedException();
    }
}