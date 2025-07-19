// Assembly-CSharp, Version=1.5.9214.33606, Culture=neutral, PublicKeyToken=null
// Verse.AI.JobDriver_AttackMelee

using System.Collections.Generic;
using CosmereRoshar.Job.Toil;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereRoshar.Job.Driver;

public class CastAbilityOnTarget : JobDriver {
    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        if (job.targetA.Thing is IAttackTarget target) {
            pawn.Map.attackTargetReservationManager.Reserve(pawn, job, target);
        }

        return true;
    }

    protected override IEnumerable<Verse.AI.Toil> MakeNewToils() {
        AddFinishAction(
            delegate {
                if (pawn.IsPlayerControlled && pawn.Drafted && !this.job.playerInterruptedForced) {
                    Verse.Thing targetThingA = TargetThingA;
                    if (targetThingA != null && targetThingA.def.autoTargetNearbyIdenticalThings) {
                        foreach (IntVec3 item in GenRadial.RadialCellsAround(TargetThingA.Position, 4f, false)
                                     .InRandomOrder()) {
                            if (item.InBounds(Map)) {
                                foreach (Verse.Thing thing2 in item.GetThingList(Map)) {
                                    if (thing2.def == TargetThingA.def &&
                                        pawn.CanReach(thing2, PathEndMode.Touch, Danger.Deadly) &&
                                        pawn.jobs.jobQueue.Count == 0) {
                                        Verse.AI.Job job = this.job.Clone();
                                        job.targetA = thing2;
                                        pawn.jobs.jobQueue.EnqueueFirst(job);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        );

        yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
        yield return CastAbility.FollowAndCastAbility(
                TargetIndex.A,
                TargetIndex.B,
                delegate {
                    Verse.Thing thing = job.GetTarget(TargetIndex.A).Thing;
                    if (job.reactingToMeleeThreat && thing is Pawn p && !p.Awake()) {
                        EndJobWith(JobCondition.InterruptForced);
                    } else if (pawn.CurJob != null && pawn.jobs.curDriver == this) {
                        if (job is not Job.CastAbilityOnTarget abilityJob ||
                            !abilityJob.abilityToCast.CanCast) {
                            return;
                        }

                        abilityJob.abilityToCast.Activate(TargetThingA, TargetThingA);
                        EndJobWith(JobCondition.Succeeded);
                    }
                }
            )
            .FailOnDespawnedOrNull(TargetIndex.A);
    }

    public override void Notify_PatherFailed() {
        if (job.attackDoorIfTargetLost) {
            Verse.Thing thing;
            using (PawnPath pawnPath = pawn.Map.pathFinder.FindPathNow(
                       pawn.Position,
                       TargetA,
                       pawn
                   )) {
                if (!pawnPath.Found) {
                    return;
                }

                thing = pawnPath.FirstBlockingBuilding(out _, pawn);
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

    public override bool IsContinuation(Verse.AI.Job j) {
        return job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
    }
}