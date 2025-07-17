// Assembly-CSharp, Version=1.5.9214.33606, Culture=neutral, PublicKeyToken=null
// Verse.AI.JobDriver_AttackMelee
using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;


namespace CosmereRoshar {
    public class Job_CastAbilityOnTarget : Job {
        public Ability abilityToCast;

        public Job_CastAbilityOnTarget() : base() { }

        public Job_CastAbilityOnTarget(JobDef def, LocalTargetInfo targetA, Ability ability) : base(def, targetA) {
            this.abilityToCast = ability;
        }
    }

    public class Toils_Cast_Ability {
        public static Toil FollowAndCastAbility(TargetIndex targetInd, TargetIndex standPositionInd, Action hitAction) {
            Toil followAndAttack = ToilMaker.MakeToil("FollowAndCastAbility");
            followAndAttack.tickAction = delegate {
                Pawn actor = followAndAttack.actor;
                Job curJob = actor.jobs.curJob;
                JobDriver curDriver = actor.jobs.curDriver;
                LocalTargetInfo target = curJob.GetTarget(targetInd);
                Thing thing = target.Thing;
                Pawn pawn = thing as Pawn;
                if (!thing.Spawned || (pawn != null && pawn.IsPsychologicallyInvisible())) {
                    curDriver.ReadyForNextToil();
                }
                else {
                    LocalTargetInfo localTargetInfo = target;
                    PathEndMode peMode = PathEndMode.Touch;
                    if (standPositionInd != 0) {
                        LocalTargetInfo target2 = curJob.GetTarget(standPositionInd);
                        if (target2.IsValid) {
                            localTargetInfo = target2;
                            peMode = PathEndMode.OnCell;
                        }
                    }

                    if (localTargetInfo != actor.pather.Destination || (!actor.pather.Moving && !actor.CanReachImmediate(target, PathEndMode.Touch))) {
                        actor.pather.StartPath(localTargetInfo, peMode);
                    }
                    else if (actor.CanReachImmediate(target, PathEndMode.Touch)) {
                        hitAction();
                    }
                }
            };
            followAndAttack.activeSkill = () => SkillDefOf.Melee;
            followAndAttack.defaultCompleteMode = ToilCompleteMode.Never;
            return followAndAttack;
        }
    }
    public class JobDriver_CastAbilityOnTarget : JobDriver {
        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            if (job.targetA.Thing is IAttackTarget target) {
                pawn.Map.attackTargetReservationManager.Reserve(pawn, job, target);
            }
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils() {
            AddFinishAction(delegate {
                if (pawn.IsPlayerControlled && pawn.Drafted && !base.job.playerInterruptedForced) {
                    Thing targetThingA = base.TargetThingA;
                    if (targetThingA != null && targetThingA.def.autoTargetNearbyIdenticalThings) {
                        foreach (IntVec3 item in GenRadial.RadialCellsAround(base.TargetThingA.Position, 4f, useCenter: false).InRandomOrder()) {
                            if (item.InBounds(base.Map)) {
                                foreach (Thing thing2 in item.GetThingList(base.Map)) {
                                    if (thing2.def == base.TargetThingA.def && pawn.CanReach(thing2, PathEndMode.Touch, Danger.Deadly) && pawn.jobs.jobQueue.Count == 0) {
                                        Job job = base.job.Clone();
                                        job.targetA = thing2;
                                        pawn.jobs.jobQueue.EnqueueFirst(job, null);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            });

            yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
            yield return Toils_Cast_Ability.FollowAndCastAbility(TargetIndex.A, TargetIndex.B, delegate {
                Thing thing = job.GetTarget(TargetIndex.A).Thing;
                if (job.reactingToMeleeThreat && thing is Pawn p && !p.Awake()) {
                    EndJobWith(JobCondition.InterruptForced);
                }
                else if (pawn.CurJob != null && pawn.jobs.curDriver == this) {

                    if (job is Job_CastAbilityOnTarget abilityJob && abilityJob.abilityToCast != null && abilityJob.abilityToCast.CanCast) {
                        abilityJob.abilityToCast.Activate(base.TargetThingA, base.TargetThingA);
                        EndJobWith(JobCondition.Succeeded);
                    }
                }
            }).FailOnDespawnedOrNull(TargetIndex.A);
        }

        public override void Notify_PatherFailed() {
            if (job.attackDoorIfTargetLost) {
                Thing thing;
                using (PawnPath pawnPath = pawn.Map.pathFinder.FindPathNow(start: pawn.Position,
                                                                           target: base.TargetA,
                                                                           pawn: pawn)) {
                    if (!pawnPath.Found) {
                        return;
                    }
                    thing = pawnPath.FirstBlockingBuilding(out var _, pawn);
                }
                if (thing != null && thing.Position.InHorDistOf(pawn.Position, 6f)) {
                    job.targetA = thing;
                    job.maxNumMeleeAttacks = Rand.RangeInclusive(2, 5);
                    job.expiryInterval = Rand.Range(2000, 4000);
                    return;
                }
            }
            base.Notify_PatherFailed();
        }

        public override bool IsContinuation(Job j) {
            return job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
        }
    }
}