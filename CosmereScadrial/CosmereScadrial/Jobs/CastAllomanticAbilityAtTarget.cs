using System;
using System.Collections.Generic;
using CosmereScadrial.Abilities.Allomancy;
using CosmereScadrial.Comps.Game;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using CosmereScadrial.Utils;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CosmereScadrial.Jobs {
    public class CastAllomanticAbilityAtTarget : JobDriver {
        private AbstractAbility cachedAbility;

        protected virtual AbstractAbility ability =>
            cachedAbility ??= pawn.abilities.GetAbility(job.ability.def) as AbstractAbility;

        protected virtual MetalBurning metalBurning => pawn.GetComp<MetalBurning>();

        private Material lineMaterial => MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.SolidColorBehind,
            ability.def.metal.color with { a = .3f });

        private AllomancyPolarity polarity => ability.def.metal.allomancy.polarity ?? AllomancyPolarity.Pushing;

        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            return pawn.Reserve(TargetA, job, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils() {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A)
                .AddFinishAction(_ => UpdateStatusToOff());

            yield return Toils_Combat.GotoCastPosition(TargetIndex.A, maxRangeFactor: 0.95f);
            yield return Toils_General.DoAtomic(() => { ability.UpdateStatus((BurningStatus)job.count); });

            var toil = ToilMaker.MakeToil(nameof(CastAllomanticAbilityAtTarget));
            toil.initAction = () => {
                if (ShouldStopJob()) {
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }

                // Ensure the burn rate is set properly
                UpdateBurnRate(ability.GetDesiredBurnRateForStatus());

                MoveThing(TargetA.Thing);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;

            yield return toil;
        }

        protected virtual bool ShouldStopJob() {
            return pawn.Downed || pawn.Dead || !AllomancyUtility.CanUseMetal(pawn, ability.metal);
        }

        protected virtual void UpdateBurnRate(float desiredBurnRate) {
            metalBurning.UpdateBurnSource(ability.metal, desiredBurnRate, ability.def);
        }

        protected virtual void UpdateStatusToOff() {
            if (ability.status != BurningStatus.Off) {
                ability.UpdateStatus(BurningStatus.Off);
            }

            UpdateBurnRate(0f);
        }

        /// <summary>
        ///     Right now, if thing hits a cell that isnt walkable, it stops.
        ///     Instead, it should add the mass of the thing in the way, and recalculate, the previous call, pushing both if the
        ///     math still works out, otherwise it should flip,
        ///     and start pushing the opposite way for the remainder of the distance
        /// </summary>
        private void MoveThing(Thing thing) {
            var surge = AllomancyUtility.GetSurgeBurn(pawn);
            surge?.Burn();

            var mass = MetalDetector.GetMass(thing);
            var pawnMass = MetalDetector.GetMass(pawn);
            (Thing, Thing) things = mass > pawnMass ? (pawn, thing) : (thing, pawn);
            var distanceBetweenThings = (things.Item2.Position - things.Item1.Position).LengthHorizontal;
            var dir = GetDirectionalOffsetFromTarget(things.Item2, things.Item1);
            var forceMultiplier = ability.GetStrength();
            var distance = Mathf.Clamp(20f / mass, 1f, 8f) * forceMultiplier * 2;
            if (polarity == AllomancyPolarity.Pulling) {
                distance = Mathf.Min(distance, distanceBetweenThings);
            }

            var destination = things.Item1.Position + dir * (int)distance;
            if (Mathf.Approximately(distance, distanceBetweenThings)) {
                destination = things.Item2.Position;
            }

            var finalPos = destination;
            for (var i = 1; i <= distance; i++) {
                var cell = things.Item1.Position + dir * i;
                if (cell.InBounds(pawn.Map) && cell.Walkable(pawn.Map)) continue;

                finalPos = things.Item1.Position + dir * (i - 1); // Stop before the obstacle
                break;
            }

            Current.Game.GetComponent<GradualMoverManager>().StartMovement(polarity, things.Item2, things.Item1,
                finalPos, Mathf.Max(5, Mathf.RoundToInt(30f / forceMultiplier)), lineMaterial);
            surge?.PostBurn();
        }

        private IntVec3 GetDirectionalOffsetFromTarget(Thing target, Thing source) {
            var offset = (source.Position.ToVector3() - target.Position.ToVector3()).normalized;
            if (polarity == AllomancyPolarity.Pulling) {
                offset = -offset;
            }

            return new IntVec3(
                (int)Math.Round(offset.x, MidpointRounding.AwayFromZero),
                0,
                (int)Math.Round(offset.z, MidpointRounding.AwayFromZero)
            );
        }
    }
}