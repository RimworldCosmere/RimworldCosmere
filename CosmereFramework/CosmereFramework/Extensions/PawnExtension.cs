using Verse;
using Verse.AI;

namespace CosmereFramework.Extensions;

public static class PawnExtension {
    public static void MaintainProximityTo(this Pawn pawn, LocalTargetInfo target, float maxDistance,
        PathEndMode endMode) {
        float distance = pawn.Position.DistanceTo(target.CenterVector3.ToIntVec3());

        if (distance > maxDistance && !pawn.pather.MovingNow) {
            // Only re-path if not already moving, avoids constant path spam
            pawn.pather.StartPath(target, endMode);
        } else if (distance <= maxDistance && pawn.pather.MovingNow) {
            // Stop if already within desired range
            pawn.pather.StopDead();
            pawn.jobs.curDriver.Notify_PatherArrived();
        }
    }
}