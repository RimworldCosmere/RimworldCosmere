using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.Def;

public interface IIdealChecker {
    bool IsSatisfied(Pawn pawn, Surgebinder surgebinder, int nextLevel);

    bool ConsummateOath(Pawn pawn, Surgebinder surgebinder, int nextLevel);

    bool HasIncompatibleTrait(Pawn pawn, int nextLevel);

    string? GetIncompatibleTraitName(Pawn pawn);

    string? GetRequirementsText(int idealIndex);
}
