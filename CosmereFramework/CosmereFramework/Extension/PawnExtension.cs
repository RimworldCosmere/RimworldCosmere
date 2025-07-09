using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CosmereFramework.Extension;

public static class PawnExtension {
    public static void MaintainProximityTo(
        this Pawn pawn,
        LocalTargetInfo target,
        float maxDistance,
        PathEndMode endMode
    ) {
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

    public static bool IsAsleep(this Pawn pawn) {
        return pawn.CurJob?.def == JobDefOf.LayDown &&
               pawn.jobs.curDriver is JobDriver_LayDown driver &&
               driver.asleep;
    }

    public static float DistanceTo(this Pawn pawn, Thing thing) {
        return pawn.DistanceTo(thing.Position);
    }

    public static float DistanceTo(this Pawn pawn, IntVec3 position) {
        return pawn.Position.DistanceTo(position);
    }

    public static List<IntVec3> GetCellsAround(this Pawn pawn, float radius, bool useCenter = false) {
        return GenRadial.RadialCellsAround(
                pawn.Position,
                Mathf.Round(Math.Min(GenRadial.MaxRadialPatternRadius, radius)),
                useCenter
            )
            .ToList();
    }
}