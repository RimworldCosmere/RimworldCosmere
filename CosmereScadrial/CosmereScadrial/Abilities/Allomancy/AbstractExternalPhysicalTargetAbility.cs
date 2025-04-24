using System;
using CosmereScadrial.Comps.Game;
using CosmereScadrial.Utils;
using RimWorld;
using UnityEngine;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereScadrial.Abilities.Allomancy {
    public abstract class AbstractExternalPhysicalTargetAbility : AbilityOtherTarget {
        protected abstract Material lineMaterial { get; }

        protected override sealed bool toggleable => false;

        public override bool CanApplyOn(LocalTargetInfo target) {
            if (!base.CanApplyOn(target) || !target.HasThing || !MetalDetector.IsCapableOfHavingMetal(target.Thing.def)) return false;

            return MetalDetector.GetMetal(target.Thing) > 0f;
        }

        public override bool Activate(LocalTargetInfo targetInfo, LocalTargetInfo dest, bool flare) {
            target = targetInfo;

            var baseActivate = base.Activate(target, dest, flare);
            if (!baseActivate || !target.HasThing) {
                return false;
            }

            MoveThing(pawn, target.Thing, flare);

            return true;
        }

        private IntVec3 GetDirectionBehind(Thing target, Thing source) {
            var directionToSource = (source.Position.ToVector3() - target.Position.ToVector3()).normalized;
            var positionBehindTarget = new IntVec3(
                (int)Math.Round(directionToSource.x, MidpointRounding.AwayFromZero),
                0,
                (int)Math.Round(directionToSource.z, MidpointRounding.AwayFromZero)
            );

            return positionBehindTarget;
        }

        private float GetMass(Thing thing) {
            var massStat = thing.GetStatValue(RimWorld.StatDefOf.Mass);
            switch (thing?.def?.category) {
                case ThingCategory.Pawn:
                    var pawn = (Pawn)thing;
                    var bodyType = pawn.story.bodyType;
                    var size = 1f;
                    if (bodyType.Equals(BodyTypeDefOf.Thin)) {
                        size = 0.8f;
                    } else if (bodyType.Equals(BodyTypeDefOf.Baby)) {
                        size = 0.2f;
                    } else if (bodyType.Equals(BodyTypeDefOf.Child)) {
                        size = 0.5f;
                    } else if (bodyType.Equals(BodyTypeDefOf.Fat)) {
                        size = 1.3f;
                    } else if (bodyType.Equals(BodyTypeDefOf.Hulk)) {
                        size = 1.5f;
                    } else if (bodyType.Equals(BodyTypeDefOf.Female)) {
                        size = 0.9f;
                    }

                    return massStat * size + MetalDetector.GetMetal(thing);
                case ThingCategory.Item:
                    return massStat * thing.stackCount;
                case ThingCategory.Building: {
                    var building = (Building)thing;
                    if (massStat <= 1f) {
                        massStat = 100;
                    }

                    // Add a 100x multiplier to this stat. For some reason, buildings dont have a mass.
                    return massStat * building.def.Size.x * building.def.Size.z * MetalDetector.GetMetal(building);
                }
                case null: return float.MaxValue;
                default:
                    return massStat;
            }
        }

        /// <summary>
        ///     Right now, if thing hits a cell that isnt walkable, it stops.
        ///     Instead, it should add the mass of the thing in the way, and recalculate, the previous call, pushing both if the
        ///     math still works out, otherwise it should flip,
        ///     and start pushing the opposite way for the remainder of the distance
        /// </summary>
        private void MoveThing(Thing pawn, Thing thing, bool flare) {
            var mass = GetMass(thing);
            var pawnMass = GetMass(pawn);
            var things = mass > pawnMass ? (pawn, thing) : (thing, pawn);
            var direction = GetDirectionBehind(things.Item2, things.Item1);
            var forceMultiplier = GetForceMultiplier(flare);
            var distance = Mathf.Clamp(20f / mass, 1f, 8f) * forceMultiplier * 2;
            var destination = things.Item1.Position + direction * (int)distance;
            var finalPos = destination;

            for (var i = 1; i <= distance; i++) {
                var cell = things.Item1.Position + direction * i;
                if (cell.InBounds(pawn.Map) && cell.Walkable(pawn.Map)) continue;

                finalPos = things.Item1.Position + direction * (i - 1); // Stop before the obstacle
                break;
            }

            Current.Game.GetComponent<GradualMoverManager>().StartMovement(things.Item2, things.Item1, finalPos, Mathf.Max(5, Mathf.RoundToInt(30f / forceMultiplier)), lineMaterial);
            Log.Verbose($"Pushed {pawn} mass={mass} forceMultiplier={forceMultiplier} distance={distance} direction={direction}");
        }

        private float GetForceMultiplier(bool flare) {
            const int multiplier = 12;
            var rawPower = pawn.GetStatValue(StatDefOf.Cosmere_Allomantic_Power);

            return multiplier * rawPower * (flare ? 2f : 1f);
        }
    }
}