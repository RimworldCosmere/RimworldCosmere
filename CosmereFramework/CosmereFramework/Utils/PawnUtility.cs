using RimWorld;
using Verse;

namespace CosmereFramework.Utils {
    public static class PawnUtility {
        public static bool IsAsleep(Pawn pawn) {
            return pawn.CurJob?.def == JobDefOf.LayDown && pawn.jobs.curDriver is JobDriver_LayDown driver && driver.asleep;
        }

        public static float DistanceBetween(Pawn pawn1, Pawn pawn2) {
            return pawn1.Position.DistanceTo(pawn2.Position);
        }
    }
}