using Cosmere.System.Scadrial.Allomancy.Hediff;
using Verse;

namespace Cosmere.System.Scadrial.Utility;

public static class AllomancyUtility {
    public static SurgeChargeHediff? GetSurgeBurn(Pawn pawn) {
        return pawn.health.hediffSet.TryGetHediff(out SurgeChargeHediff hediff) ? hediff : null;
    }
}