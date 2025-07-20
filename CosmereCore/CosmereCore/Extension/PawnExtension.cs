using Cosmere.Core.Util;
using Verse;

namespace Cosmere.Core.Extension;

public static class PawnExtension {
    public static bool IsShieldedAgainstInvestiture(this Pawn pawn) {
        return InvestitureDetector.IsShielded(pawn);
    }
}