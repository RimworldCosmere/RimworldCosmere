using RimWorld;
using Verse;

namespace CosmereFramework.Utils {
    public static class PawnUtility {
        public static bool IsAsleep(Pawn pawn) {
            return pawn.CurJob?.def == JobDefOf.LayDown && pawn.jobs.curDriver is JobDriver_LayDown driver && driver.asleep;
        }
    }
}