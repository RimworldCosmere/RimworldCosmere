using System.Collections.Generic;
using CosmereScadrial.Abilities.Allomancy;
using CosmereScadrial.Comps.Hediffs;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Utils;
using Verse;
using Verse.AI;
using HediffUtility = CosmereScadrial.Utils.HediffUtility;
using PawnUtility = CosmereFramework.Utils.PawnUtility;

namespace CosmereScadrial.Jobs {
    public class MaintainAllomanticTarget : JobDriver {
        protected virtual Pawn targetPawn => TargetA.Pawn;

        protected virtual AbstractAbility ability => (AbstractAbility)job.source;

        protected virtual MetalBurning metalBurning => pawn.GetComp<MetalBurning>();

        protected virtual bool targetIsPawn => targetPawn != null;

        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            return pawn.Reserve(targetPawn, job, errorOnFailed: errorOnFailed, maxPawns: int.MaxValue, stackCount: 1,
                ignoreOtherReservations: true);
        }

        protected override IEnumerable<Toil> MakeNewToils() {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A)
                .FailOnDowned(TargetIndex.A);

            var gotoThing = Toils_General.Do(MaintainProximity);
            gotoThing.defaultCompleteMode = ToilCompleteMode.PatherArrival;

            yield return Toils_General.DoAtomic(() => { ability.UpdateStatus((BurningStatus)job.count); });

            var toil = new Toil {
                defaultCompleteMode = ToilCompleteMode.Never,
                tickAction = () => {
                    if (ShouldStopJob()) {
                        ability.UpdateStatus(BurningStatus.Off);
                        EndJobWith(JobCondition.Incompletable);
                        return;
                    }

                    // Maintain proximity
                    MaintainProximity();

                    // If we arent close enough to the pawn, update burnrate to 0, and get closer
                    if (PawnUtility.DistanceBetween(pawn, targetPawn) > ability.def.verbProperties.range) {
                        UpdateBurnRate(0f);
                        return;
                    }

                    var surge = AllomancyUtility.GetSurgeBurn(pawn);
                    surge?.Burn();

                    // Ensure the burn rate is set properly
                    MaintainBurnRate();

                    // Apply and maintain effect
                    MaintainEffectOnTarget();

                    if (surge == null) return;
                    surge.PostBurn();
                    EndJobWith(JobCondition.Succeeded);
                },
            };
            toil.AddFinishAction(UpdateStatusToOff);

            yield return toil;
        }

        protected virtual bool ShouldStopJob() {
            if (pawn.Downed || pawn.Dead || targetIsPawn && (targetPawn.Dead || targetPawn.Downed)) {
                return true;
            }

            if (!AllomancyUtility.CanUseMetal(pawn, ability.metal)) return true;

            return !pawn.CanReach(TargetA, targetIsPawn ? PathEndMode.Touch : PathEndMode.OnCell, Danger.None);
        }

        protected virtual void MaintainProximity() {
            var distance = pawn.Position.DistanceTo(TargetA.Cell);

            if (distance > job.followRadius && !pawn.pather.MovingNow) {
                // Only re-path if not already moving, avoids constant path spam
                pawn.pather.StartPath(TargetA, targetIsPawn ? PathEndMode.Touch : PathEndMode.OnCell);
            } else if (distance <= job.followRadius && pawn.pather.MovingNow) {
                // Stop if already within desired range
                pawn.pather.StopDead();
                pawn.jobs.curDriver.Notify_PatherArrived();
            }
        }

        protected virtual void MaintainBurnRate() {
            UpdateBurnRate(ability.GetDesiredBurnRateForStatus());
        }

        protected virtual void MaintainEffectOnTarget() {
            var hediff = HediffUtility.GetOrAddHediff(pawn, targetPawn, ability, ability.def);

            hediff.TryGetComp<DisappearsScaled>()?.CompPostMake();
        }

        protected virtual void UpdateStatusToOff() {
            if (ability.status != BurningStatus.Off) {
                ability.UpdateStatus(BurningStatus.Off);
            }
        }

        protected virtual void UpdateBurnRate(float desiredBurnRate) {
            metalBurning.UpdateBurnSource(ability.metal, desiredBurnRate, ability.def);
        }
    }
}