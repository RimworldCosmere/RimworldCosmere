using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereRoshar.Job.Toil;

public class CastAbility {
    public static Verse.AI.Toil FollowAndCastAbility(
        TargetIndex targetInd,
        TargetIndex standPositionInd,
        Action hitAction
    ) {
        Verse.AI.Toil followAndAttack = ToilMaker.MakeToil();
        followAndAttack.tickAction = delegate {
            Pawn actor = followAndAttack.actor;
            Verse.AI.Job curJob = actor.jobs.curJob;
            JobDriver curDriver = actor.jobs.curDriver;
            LocalTargetInfo target = curJob.GetTarget(targetInd);
            Verse.Thing thing = target.Thing;
            Pawn pawn = thing as Pawn;
            if (!thing.Spawned || pawn != null && pawn.IsPsychologicallyInvisible()) {
                curDriver.ReadyForNextToil();
            } else {
                LocalTargetInfo localTargetInfo = target;
                PathEndMode peMode = PathEndMode.Touch;
                if (standPositionInd != 0) {
                    LocalTargetInfo target2 = curJob.GetTarget(standPositionInd);
                    if (target2.IsValid) {
                        localTargetInfo = target2;
                        peMode = PathEndMode.OnCell;
                    }
                }

                if (localTargetInfo != actor.pather.Destination ||
                    !actor.pather.Moving && !actor.CanReachImmediate(target, PathEndMode.Touch)) {
                    actor.pather.StartPath(localTargetInfo, peMode);
                } else if (actor.CanReachImmediate(target, PathEndMode.Touch)) {
                    hitAction();
                }
            }
        };
        followAndAttack.activeSkill = () => SkillDefOf.Melee;
        followAndAttack.defaultCompleteMode = ToilCompleteMode.Never;
        return followAndAttack;
    }
}