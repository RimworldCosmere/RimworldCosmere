using System;
using System.Collections.Generic;
using CosmereScadrial.Abilities.Allomancy;
using CosmereScadrial.Comps.Game;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using CosmereScadrial.Utils;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CosmereScadrial.Jobs {
    public class CastAllomanticAbilityAtTarget : JobDriver {
        protected virtual AbstractAbility ability => pawn.abilities.GetAbility(job.ability.def) as AbstractAbility;

        protected virtual MetalBurning metalBurning => pawn.GetComp<MetalBurning>();

        private bool isFlare => (BurningStatus)job.count == BurningStatus.Flaring;

        private Material lineMaterial => MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.SolidColorBehind, ability.def.metal.color with { a = .3f });

        private AllomancyPolarity polarity => ability.def.metal.allomancy.polarity ?? AllomancyPolarity.Pushing;

        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            return pawn.Reserve(TargetA, job, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils() {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A)
                .AddFinishAction(_ => UpdateBurnRate(0f));

            yield return Toils_Combat.GotoCastPosition(TargetIndex.A, maxRangeFactor: 0.95f);

            var toil = ToilMaker.MakeToil(nameof(CastAllomanticAbilityAtTarget));
            toil.initAction = () => {
                // Ensure the burn rate is set properly
                DoBurn();

                MoveThing(TargetA.Thing, isFlare);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            //toil.AddFinishAction(() => UpdateBurnRate(0f));

            yield return toil;
        }

        protected virtual void DoBurn() {
            UpdateBurnRate(ability.GetDesiredBurnRateForStatus());
        }

        protected virtual void UpdateBurnRate(float desiredBurnRate) {
            if (!Mathf.Approximately(metalBurning.GetBurnRateForMetalDef(ability.metal, ability.def), desiredBurnRate)) {
                metalBurning.UpdateBurnSource(ability.metal, desiredBurnRate, ability.def);
            }
        }

        /// <summary>
        ///     Right now, if thing hits a cell that isnt walkable, it stops.
        ///     Instead, it should add the mass of the thing in the way, and recalculate, the previous call, pushing both if the
        ///     math still works out, otherwise it should flip,
        ///     and start pushing the opposite way for the remainder of the distance
        /// </summary>
        private void MoveThing(Thing thing, bool flare) {
            var mass = MetalDetector.GetMass(thing);
            var pawnMass = MetalDetector.GetMass(pawn);
            (Thing, Thing) things = mass > pawnMass ? (pawn, thing) : (thing, pawn);
            var distanceBetweenThings = (things.Item2.Position - things.Item1.Position).LengthHorizontal;
            var dir = GetDirectionalOffsetFromTarget(things.Item2, things.Item1);
            var forceMultiplier = GetForceMultiplier(flare);
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

            Current.Game.GetComponent<GradualMoverManager>().StartMovement(polarity, things.Item2, things.Item1, finalPos, Mathf.Max(5, Mathf.RoundToInt(30f / forceMultiplier)), lineMaterial);
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

        private float GetForceMultiplier(bool flare) {
            const int multiplier = 12;
            var rawPower = pawn.GetStatValue(StatDefOf.Cosmere_Allomantic_Power);

            return multiplier * rawPower * (flare ? 2f : 1f);
        }
    }
}