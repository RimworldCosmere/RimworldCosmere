using Cosmere.Roshar.Def;
using Cosmere.Roshar.Gene;
using Verse;

namespace Cosmere.Roshar.Surgebinding.IdealChecker;

public abstract class AbstractIdealChecker(RadiantOrderDef def) {
    private readonly RadiantOrderDef def = def;

    public abstract bool IsSatisfied(Pawn pawn, Surgebinder surgebinder, int nextLevel);

    public abstract bool Satisfy(Pawn pawn, Surgebinder surgebinder, int nextLevel);
}