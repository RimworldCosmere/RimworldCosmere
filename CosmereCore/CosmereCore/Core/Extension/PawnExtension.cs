using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Cosmere.Core.Extension;

public static class PawnExtension {
    public static InvestitureHolder? GetInvestiture(this Pawn pawn) {
        return pawn.TryGetComp<InvestitureHolder>();
    }

    public static bool IsShieldedAgainstInvestiture(this Pawn pawn) {
        return InvestitureDetector.IsShielded(pawn);
    }

    public static void MaintainProximityTo(
        this Pawn pawn,
        LocalTargetInfo target,
        float maxDistance,
        PathEndMode endMode
    ) {
        float distance = pawn.Position.DistanceTo(target.CenterVector3.ToIntVec3());

        if (distance > maxDistance && !pawn.pather.MovingNow) {
            pawn.pather.StartPath(target, endMode);
        }
        else if (distance <= maxDistance && pawn.pather.MovingNow) {
            pawn.pather.StopDead();
            pawn.jobs.curDriver.Notify_PatherArrived();
        }
    }

    public static bool IsAsleep(this Pawn pawn) {
        return pawn.CurJob?.def == RimWorld.JobDefOf.LayDown &&
               pawn.jobs.curDriver is JobDriver_LayDown { asleep: true };
    }

    public static float DistanceTo(this Pawn pawn, Verse.Thing thing) {
        return pawn.DistanceTo(thing.Position);
    }

    public static float DistanceTo(this Pawn pawn, IntVec3 position) {
        return pawn.Position.DistanceTo(position);
    }

    public static List<IntVec3> GetCellsAround(this Pawn pawn, float radius, bool useCenter = false) {
        float clampedRadius = Mathf.Round(Mathf.Min(GenRadial.MaxRadialPatternRadius - .01f, radius));
        List<IntVec3> cells = new List<IntVec3>(GenRadial.NumCellsInRadius(clampedRadius));
        foreach (IntVec3 cell in GenRadial.RadialCellsAround(pawn.Position, clampedRadius, useCenter)) {
            if (cell.InBounds(pawn.Map)) cells.Add(cell);
        }

        return cells;
    }

    public static bool TryGetAbility<T>(this Pawn pawn, AbilityDef def, out T? ability)
        where T : RimWorld.Ability {
        ability = pawn.abilities.GetAbility(def) as T;
        return ability != null;
    }

    public static T? GetAbility<T>(this Pawn pawn, AbilityDef def)
        where T : RimWorld.Ability {
        return pawn.abilities.GetAbility(def) as T;
    }
}