using Cosmere.Scadrial.Allomancy.Hediff;
using Verse;

namespace Cosmere.Scadrial.Util;

public static class AllomancyUtility {
    public static SurgeChargeHediff? GetSurgeBurn(Pawn pawn) {
        return pawn.health.hediffSet.TryGetHediff(out SurgeChargeHediff hediff) ? hediff : null;
    }
}