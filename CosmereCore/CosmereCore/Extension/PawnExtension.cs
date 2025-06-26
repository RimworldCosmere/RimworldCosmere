using CosmereCore.Util;
using Verse;

namespace CosmereCore.Extension;

public static class PawnExtension {
    public static bool IsShieldedAgainstInvestiture(this Pawn pawn) {
        return InvestitureDetector.IsShielded(pawn);
    }
}