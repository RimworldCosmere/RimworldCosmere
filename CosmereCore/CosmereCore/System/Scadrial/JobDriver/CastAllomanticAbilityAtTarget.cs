using System;
using Cosmere.Core.Ability;
using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Allomancy.Comp.Game;
using Cosmere.System.Scadrial.Allomancy.Hediff;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Utility;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.JobDriver;

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

            Status activeStatus = ability.nextStatus ?? BurningStatus.Burning;
            ability.UpdateStatus(activeStatus);

            UpdateBurnRate(ability.GetDesiredBurnRateForStatus());

            MoveThing(TargetA.Thing, true);

            ability.UpdateStatus(BurningStatus.Off);
        };
        toil.defaultCompleteMode = ToilCompleteMode.Instant;

        yield return toil;
    }

    // TODO: Add distance from target to equation of push strength
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
        float mass = thing.GetStatValue(RimWorld.StatDefOf.Mass) * thing.stackCount;
        float pawnMass = pawn.GetStatValue(RimWorld.StatDefOf.Mass) +
                         MassUtility.GearAndInventoryMass(pawn) * forceMultiplier;
        float massDifference = Mathf.Abs(pawnMass - mass);
        if (mass > pawnMass && !movePawn) return;

        (Verse.Thing, Verse.Thing) things = mass > pawnMass ? (pawn, thing) : (thing, pawn);
        float distanceBetweenThings = (things.Item2.Position - things.Item1.Position).LengthHorizontal;
        IntVec3 dir = GetDirectionalOffsetFromTarget(things.Item2, things.Item1);
        float distance = Mathf.Lerp(0, massDifference, .333333f) * forceMultiplier;
        if (polarity == AllomancyPolarity.Pulling) distance = Mathf.Min(distance, distanceBetweenThings);
        if (distance > 0.05f) distance = Mathf.Max(distance, 1f);

        int distanceTiles = Mathf.RoundToInt(distance);
        IntVec3 destination = things.Item1.Position + dir * distanceTiles;
        if (Mathf.Approximately(distance, distanceBetweenThings)) destination = things.Item2.Position;

        IntVec3 finalPos = destination;
        for (int i = 1; i <= distanceTiles; i++) {
            IntVec3 cell = things.Item1.Position + dir * i;
            if (cell.InBounds(pawn.Map) && cell.Walkable(pawn.Map)) continue;

            finalPos = things.Item1.Position + dir * (i - 1);
            break;
        }

        int duration = Mathf.RoundToInt(
            Mathf.Lerp(GenTicks.TicksPerRealSecond / 2f, massDifference, 30f / forceMultiplier / 100)
        );
        Current.Game.GetComponent<GradualMoverManager>()
            .StartMovement(
                polarity,
                things.Item2,
                things.Item1,
                finalPos,
                duration,
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