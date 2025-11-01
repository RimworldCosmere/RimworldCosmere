using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.IdealChecker;

public abstract class AbstractIdealChecker(RadiantOrderDef def) {
    private readonly RadiantOrderDef def = def;

    public abstract bool IsSatisfied(Pawn pawn, Surgebinder surgebinder, int nextLevel);

    public abstract bool Satisfy(Pawn pawn, Surgebinder surgebinder, int nextLevel);
}