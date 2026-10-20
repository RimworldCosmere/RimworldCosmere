using System;
using System.Diagnostics.CodeAnalysis;
using Cosmere.Core.Ability;
using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Allomancy;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Allomancy.Comp.Game;
using Cosmere.System.Scadrial.Allomancy.Hediff;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.JobDriver;

public class CastAllomanticAbilityAtTarget : AllomanticJobDriver {
    private Material LineMaterial => MaterialPool.MatFrom(
        GenDraw.LineTexPath,
        ShaderDatabase.SolidColorBehind,
        Ability.Def.metal.color with { a = .3f }
    );

    private AllomancyPolarity Polarity => Ability.Def.metal.allomancy!.polarity ?? AllomancyPolarity.Pushing;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return pawn.Reserve(TargetA, job, errorOnFailed: errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        foreach (Toil baseToil in base.MakeNewToils()) yield return baseToil;

        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);

        Toil? toil = ToilMaker.MakeToil(nameof(CastAllomanticAbilityAtTarget));
        toil.initAction = () => {
            if (ShouldStopJob()) {
                Ability.UpdateStatus(BurningStatus.Off);
                EndJobWith(JobCondition.Incompletable);
                return;
            }

            Status activeStatus = Ability.NextStatus ?? BurningStatus.Burning;
            Ability.UpdateStatus(activeStatus);

            UpdateBurnRate(Ability.GetDesiredBurnRateForStatus());

            MoveThing(TargetA.Thing, true);

            Ability.UpdateStatus(BurningStatus.Off);
        };
        toil.defaultCompleteMode = ToilCompleteMode.Instant;

        yield return toil;
    }

    private void ShoveEveryMetalInRange(Verse.Thing origin) {
        IEnumerable<IntVec3>? cells = GenRadial.RadialCellsAround(
            origin.Position,
            Mathf.Round(Math.Min(GenRadial.MaxRadialPatternRadius, Ability.verb.EffectiveRange)),
            false
        );
        foreach (IntVec3 cell in cells) {
            foreach (Verse.Thing? newThing in cell.GetThingList(pawn.Map).Where(x => MetalDetector.HasMetal(x))) {
                MoveThing(newThing, false);
            }
        }
    }

    private IntVec3 FirstBlockedStop(IntVec3 start, IntVec3 dir, int distanceTiles, IntVec3 destination) {
        for (int i = 1; i <= distanceTiles; i++) {
            IntVec3 cell = start + dir * i;
            if (cell.InBounds(pawn.Map) && cell.Walkable(pawn.Map)) continue;

            return start + dir * (i - 1);
        }

        return destination;
    }

    // TODO(push strength still ignores range): distance falloff

    /// <summary>
    ///     Right now, if thing hits a cell that isnt walkable, it stops.
    ///     Instead, it should add the mass of the thing in the way, and recalculate, the previous call, pushing both if the
    ///     math still works out, otherwise it should flip,
    ///     and start pushing the opposite way for the remainder of the distance.
    /// </summary>
    [SuppressMessage("Minor Code Smell", "S1135", Justification = "the TODO on this method is open work, not a note: a shove's distance is still mass-only, so a target at the edge of range is thrown exactly as hard as one in the next cell")]
    private void MoveThing(Verse.Thing thing, bool movePawn) {
        SurgeChargeHediff? surge = AllomancyUtility.FindSurgeChargeHediff(pawn);
        surge?.Burn();

        if (thing == pawn) {
            ShoveEveryMetalInRange(thing);
            return;
        }

        float forceMultiplier = Ability.GetStrength();
        float pawnMass = pawn.GetStatValue(RimWorld.StatDefOf.Mass) +
                         MassUtility.GearAndInventoryMass(pawn) * forceMultiplier;

        // a spawned building is bolted down - moving it via Position leaves reachability stale, stranding pawns.
        bool anchored = thing.Spawned && thing.def.category == ThingCategory.Building;
        float mass = AllomanticShove.EffectiveTargetMass(
            thing.GetStatValue(RimWorld.StatDefOf.Mass) * thing.stackCount,
            pawnMass,
            anchored
        );
        float massDifference = Mathf.Abs(pawnMass - mass);
        bool movesCaster = AllomanticShove.MovesCaster(mass, pawnMass);
        if (movesCaster && !movePawn) return;

        (Verse.Thing, Verse.Thing) things = movesCaster ? (pawn, thing) : (thing, pawn);
        float distanceBetweenThings = (things.Item2.Position - things.Item1.Position).LengthHorizontal;
        IntVec3 dir = GetDirectionalOffsetFromTarget(things.Item2, things.Item1);
        float distance = Mathf.Lerp(0, massDifference, .333333f) * forceMultiplier;
        if (Polarity == AllomancyPolarity.Pulling) distance = Mathf.Min(distance, distanceBetweenThings);
        if (distance > 0.05f) distance = Mathf.Max(distance, 1f);

        int distanceTiles = Mathf.RoundToInt(distance);
        IntVec3 destination = things.Item1.Position + dir * distanceTiles;
        if (Mathf.Approximately(distance, distanceBetweenThings)) destination = things.Item2.Position;

        IntVec3 finalPos = FirstBlockedStop(things.Item1.Position, dir, distanceTiles, destination);

        // a shove resolving onto the caster's own cell would pin them under the mover while dust plays.
        if (finalPos != things.Item1.Position) {
            int duration = Mathf.RoundToInt(
                Mathf.Lerp(GenTicks.TicksPerRealSecond / 2f, massDifference, 30f / forceMultiplier / 100)
            );
            Current.Game.GetComponent<GradualMoverManager>()
                .StartMovement(
                    Polarity,
                    things.Item2,
                    things.Item1,
                    finalPos,
                    duration,
                    LineMaterial
                );
        }

        surge?.PostBurn();
    }

    private IntVec3 GetDirectionalOffsetFromTarget(Verse.Thing target, Verse.Thing source) {
        Vector3 offset = (source.Position.ToVector3() - target.Position.ToVector3()).normalized;
        if (Polarity == AllomancyPolarity.Pulling) offset = -offset;

        return new IntVec3(
            (int)Math.Round(offset.x, MidpointRounding.AwayFromZero),
            0,
            (int)Math.Round(offset.z, MidpointRounding.AwayFromZero)
        );
    }
}
