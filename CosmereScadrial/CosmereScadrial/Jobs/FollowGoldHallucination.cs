using System.Collections.Generic;
using CosmereScadrial.Abilities.Allomancy;
using Verse;
using Verse.AI;

namespace CosmereScadrial.Jobs {
    public class FollowGoldHallucination : JobDriver {
        protected virtual Pawn hallucination => TargetA.Pawn;

        protected virtual AbstractAbility ability => pawn.abilities.GetAbility(job.ability.def) as AbstractAbility;

        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            return pawn.Reserve(pawn, job, errorOnFailed: errorOnFailed, maxPawns: int.MaxValue, stackCount: 1,
                ignoreOtherReservations: true);
        }

        protected override IEnumerable<Toil> MakeNewToils() {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A)
                .FailOnDowned(TargetIndex.A)
                .AddFinishAction(UpdateStatusToOff);

            var gotoThing = Toils_General.Do(MaintainProximity);
            gotoThing.defaultCompleteMode = ToilCompleteMode.PatherArrival;

            var toil = new Toil {
                defaultCompleteMode = ToilCompleteMode.Never,
                tickAction = () => {
                    if (ShouldStopJob()) {
                        EndJobWith(JobCondition.Incompletable);
                        return;
                    }

                    // Maintain proximity
                    MaintainProximity();
                },
            };

            yield return toil;
        }

        protected virtual bool ShouldStopJob() {
            if (pawn.Downed || pawn.Dead) {
                return true;
            }

            return !pawn.CanReach(TargetA, PathEndMode.ClosestTouch, Danger.None);
        }

        protected virtual void MaintainProximity() {
            var distance = pawn.Position.DistanceTo(hallucination.Position);

            if (distance > job.followRadius && !pawn.pather.MovingNow) {
                // Only re-path if not already moving, avoids constant path spam
                pawn.pather.StartPath(TargetA, PathEndMode.InteractionCell);
            } else if (distance <= job.followRadius && pawn.pather.MovingNow) {
                // Stop if already within desired range
                pawn.pather.StopDead();
                pawn.jobs.curDriver.Notify_PatherArrived();
            }
        }

        protected virtual void UpdateStatusToOff(JobCondition jobCondition) {
            if (ability.status != BurningStatus.Off) {
                ability.UpdateStatus(BurningStatus.Off);
            }
        }
    }
}