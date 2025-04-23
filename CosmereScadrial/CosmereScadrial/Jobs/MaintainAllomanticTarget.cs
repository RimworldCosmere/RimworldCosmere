using System.Collections.Generic;
using CosmereScadrial.Abilities;
using CosmereScadrial.Abilities.Hediffs.Comps;
using CosmereScadrial.Comps.Things;
using UnityEngine;
using Verse;
using Verse.AI;
using HediffUtility = CosmereScadrial.Utils.HediffUtility;
using PawnUtility = CosmereFramework.Utils.PawnUtility;

namespace CosmereScadrial.Jobs {
    public class MaintainAllomanticTarget : JobDriver {
        protected virtual Pawn targetPawn => TargetA.Pawn;

        protected virtual AbstractAllomanticAbility ability => pawn.abilities.GetAbility(job.ability.def) as AbstractAllomanticAbility;

        protected virtual MetalBurning metalBurning => pawn.GetComp<MetalBurning>();

        protected virtual bool targetIsPawn => targetPawn != null;

        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            return pawn.Reserve(targetPawn, job, errorOnFailed: errorOnFailed, maxPawns: int.MaxValue, stackCount: 1, ignoreOtherReservations: true);
        }

        protected override IEnumerable<Toil> MakeNewToils() {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A)
                .FailOnDowned(TargetIndex.A)
                .AddFinishAction(UpdateStatusToOff);

            var gotoThing = Toils_General.Do(MaintainProximity);
            gotoThing.defaultCompleteMode = ToilCompleteMode.PatherArrival;

            yield return Toils_General.DoAtomic(() => { ability.UpdateStatus((BurningStatus)job.count); });

            var toil = new Toil {
                defaultCompleteMode = ToilCompleteMode.Never,
                tickAction = () => {
                    if (ShouldStopJob()) {
                        EndJobWith(JobCondition.Incompletable);
                        return;
                    }

                    // Maintain proximity
                    MaintainProximity();

                    // Ensure the burn rate is set properly
                    MaintainBurnRate();

                    // Apply and maintain effect
                    MaintainEffectOnTarget();
                },
            };
            toil.AddFinishAction(UpdateStatusToOff);

            yield return toil;
        }

        protected virtual bool ShouldStopJob() {
            if (pawn.Downed || pawn.Dead || targetIsPawn && (targetPawn.Dead || targetPawn.Downed)) {
                return false;
            }

            return !pawn.CanReach(TargetA, targetIsPawn ? PathEndMode.Touch : PathEndMode.OnCell, Danger.None);
        }

        protected virtual void MaintainProximity() {
            var followRadius = ability.def.maintenance.followRadius;
            var distance = pawn.Position.DistanceTo(TargetA.Cell);

            if (distance > followRadius && pawn.pather.MovingNow == false) {
                // Only re-path if not already moving, avoids constant path spam
                pawn.pather.StartPath(TargetA, targetIsPawn ? PathEndMode.Touch : PathEndMode.OnCell);
            } else if (distance <= followRadius && pawn.pather.MovingNow) {
                // Stop if already within desired range
                pawn.pather.StopDead();
                pawn.jobs.curDriver.Notify_PatherArrived();
            }
        }

        protected virtual void MaintainBurnRate() {
            // If we arent close enough to the pawn, update burnrate to 0, and get closer
            if (PawnUtility.DistanceBetween(pawn, targetPawn) > ability.def.verbProperties.range) {
                UpdateBurnRate(0f);
                return;
            }

            // If we are close enough, get the desired burn rate based on the status.
            UpdateBurnRate(ability.GetDesiredBurnRateForStatus());
        }

        protected virtual void MaintainEffectOnTarget() {
            var hediff = HediffUtility.GetOrAddHediff(pawn, targetPawn, ability, ability.def);

            hediff.TryGetComp<DisappearsScaled>()?.CompPostMake();
        }

        protected virtual void UpdateStatusToOff() {
            ability.UpdateStatus(BurningStatus.Off);
        }

        protected virtual void UpdateStatusToOff(JobCondition jobCondition) {
            if (ability.status != BurningStatus.Off) {
                UpdateStatusToOff();
            }
        }

        protected virtual void UpdateBurnRate(float desiredBurnRate) {
            if (!Mathf.Approximately(metalBurning.GetBurnRateForMetalDef(ability.metal, ability.def), desiredBurnRate)) {
                metalBurning.UpdateBurnSource(ability.metal, desiredBurnRate, ability.def);
            }
        }
    }
}