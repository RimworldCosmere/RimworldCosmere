using System;
using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Allomancy.Ability;
using CosmereScadrial.Allomancy.Comp.Game;
using CosmereScadrial.Allomancy.Hediff;
using CosmereScadrial.Def;
using CosmereScadrial.Util;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CosmereScadrial.JobDriver;

public class CastAllomanticAbilityAtTarget : AllomanticJobDriver {
    private Material lineMaterial => MaterialPool.MatFrom(
        GenDraw.LineTexPath,
        ShaderDatabase.SolidColorBehind,
        ability.def.metal.color with { a = .3f }
    );

    private AllomancyPolarity polarity => ability.def.metal.allomancy!.polarity ?? AllomancyPolarity.Pushing;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return pawn.Reserve(TargetA, job, errorOnFailed: errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        foreach (Toil baseToil in base.MakeNewToils()) yield return baseToil;

        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);

        Toil? toil = ToilMaker.MakeToil(nameof(CastAllomanticAbilityAtTarget));
        toil.initAction = () => {
            if (ShouldStopJob()) {
                ability.UpdateStatus(BurningStatus.Off);
                EndJobWith(JobCondition.Incompletable);
                return;
            }

            // Ensure the burn rate is set properly
            UpdateBurnRate(ability.GetDesiredBurnRateForStatus());

            MoveThing(TargetA.Thing, true);

            ability.UpdateStatus(BurningStatus.Off);
        };
        toil.defaultCompleteMode = ToilCompleteMode.Instant;

        yield return toil;
    }

    // @todo Add distance from target to equation of push strength
    /// <summary>
    ///     Right now, if thing hits a cell that isnt walkable, it stops.
    ///     Instead, it should add the mass of the thing in the way, and recalculate, the previous call, pushing both if the
    ///     math still works out, otherwise it should flip,
    ///     and start pushing the opposite way for the remainder of the distance
    /// </summary>
    private void MoveThing(Verse.Thing thing, bool movePawn) {
        SurgeChargeHediff? surge = AllomancyUtility.GetSurgeBurn(pawn);
        surge?.Burn();

        if (thing == pawn) {
            // Get everything metal in a radius around self (same radius as the ability range)
            // Use MoveThing(newThing, false) on them
            IEnumerable<IntVec3>? cells = GenRadial.RadialCellsAround(
                thing.Position,
                Mathf.Round(Math.Min(GenRadial.MaxRadialPatternRadius, ability.verb.EffectiveRange)),
                false
            );
            foreach (IntVec3 cell in cells) {
                foreach (Verse.Thing? newThing in cell.GetThingList(pawn.Map).Where(x => MetalDetector.HasMetal(x))) {
                    MoveThing(newThing, false);
                }
            }

            return;
        }

        float forceMultiplier = ability.GetStrength();
        float mass = MetalDetector.GetMass(thing);
        float pawnMass = MetalDetector.GetMass(pawn) * forceMultiplier;
        if (mass > pawnMass && !movePawn) return;

        (Verse.Thing, Verse.Thing) things = mass > pawnMass ? (pawn, thing) : (thing, pawn);
        float distanceBetweenThings = (things.Item2.Position - things.Item1.Position).LengthHorizontal;
        IntVec3 dir = GetDirectionalOffsetFromTarget(things.Item2, things.Item1);
        float distance = Mathf.Clamp(20f / mass, 1f, 8f) * forceMultiplier * 2;
        if (polarity == AllomancyPolarity.Pulling) distance = Mathf.Min(distance, distanceBetweenThings);


        IntVec3 destination = things.Item1.Position + dir * (int)distance;
        if (Mathf.Approximately(distance, distanceBetweenThings)) destination = things.Item2.Position;

        IntVec3 finalPos = destination;
        for (int i = 1; i <= distance; i++) {
            IntVec3 cell = things.Item1.Position + dir * i;
            if (cell.InBounds(pawn.Map) && cell.Walkable(pawn.Map)) continue;

            finalPos = things.Item1.Position + dir * (i - 1); // Stop before the obstacle
            break;
        }

        Current.Game.GetComponent<GradualMoverManager>()
            .StartMovement(
                polarity,
                things.Item2,
                things.Item1,
                finalPos,
                Mathf.Max(5, Mathf.RoundToInt(30f / forceMultiplier)),
                lineMaterial
            );
        surge?.PostBurn();
    }

    private IntVec3 GetDirectionalOffsetFromTarget(Verse.Thing target, Verse.Thing source) {
        Vector3 offset = (source.Position.ToVector3() - target.Position.ToVector3()).normalized;
        if (polarity == AllomancyPolarity.Pulling) offset = -offset;

        return new IntVec3(
            (int)Math.Round(offset.x, MidpointRounding.AwayFromZero),
            0,
            (int)Math.Round(offset.z, MidpointRounding.AwayFromZero)
        );
    }
}