using Verse;

namespace Cosmere.Core.Framework;

public static class InvestitureBlockingHediffRegistry {
    private static readonly List<HediffDef> blockingHediffs = [];

    public static void Register(HediffDef hediff) {
        blockingHediffs.Add(hediff);
    }

    public static bool IsBlocked(Pawn pawn) {
        for (int i = 0; i < blockingHediffs.Count; i++) {
            if (pawn.health.hediffSet.HasHediff(blockingHediffs[i])) return true;
        }

        return false;
    }
}
