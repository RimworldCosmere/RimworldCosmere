using Cosmere;
using Cosmere.Core.Comp.Thing;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Utility;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.GameCondition;

/**
 * @TODO Force the weather here?
 * @TODO Draw a warning at the top
 * @TODO Encorage pawns to go inside? Probably a workgiver?
 */
public class Highstorm : RimWorld.GameCondition {
    private const float totalInvestitureToAbsorbPerItem = 2000f;

    private static readonly SimpleCurve stormIntensityCurve = [
        new CurvePoint(0f, 0), // Start of storm: 0% intensity
        new CurvePoint(0.4f, .75f), // Mid-storm: 100% intensity
        new CurvePoint(0.5f, 0f), // Mid-storm: 100% intensity
        new CurvePoint(0.6f, .75f), // Mid-storm: 100% intensity
        new CurvePoint(1f, 0),
    ];

    private static readonly List<(IntVec3 offset, float weight)> weightedOffsets = [
        (IntVec3.West, 5f),
        (IntVec3.NorthWest, 3f),
        (IntVec3.SouthWest, 3f),
        (IntVec3.North, 1f),
        (IntVec3.South, 1f),
    ];

    private readonly Verse.Thing highstorm = ThingMaker.MakeThing(ThingDefOf.Cosmere_Roshar_Thing_Highstorm);

    private readonly int tickInterval = 30;
    private float scaledCurve;

    private float investitureToAbsorb => totalInvestitureToAbsorbPerItem / Duration * tickInterval * scaledCurve;

    private static IntVec3 GetRandomStormOffset(bool timesTwo) {
        float totalWeight = weightedOffsets.Sum(entry => entry.weight);
        float choice = Rand.Range(0f, totalWeight);
        int multiplier = timesTwo ? 2 : 1;

        float cumulative = 0f;
        foreach ((IntVec3 offset, float weight) in weightedOffsets) {
            cumulative += weight;
            if (choice <= cumulative) {
                return offset * multiplier;
            }
        }

        // fallback — shouldn't hit
        return IntVec3.West * multiplier;
    }

    public override void End() {
        base.End();
        StormShelterManager.ClearCache();
        SingleMap?.weatherManager.TransitionTo(WeatherDefOf.FoggyRain);
        highstorm.Destroy();
    }

    public override float MinWindSpeed() {
        return scaledCurve;
    }

    public override void GameConditionTick() {
        base.GameConditionTick();
        ModExtension.Highstorm? ext = def.GetModExtension<ModExtension.Highstorm>();
        if (ext == null) return;
        if (StormShelterManager.FirstTickOfHighstorm) {
            Map currentMap = Find.CurrentMap;
            if (currentMap != null) {
                StormShelterManager.RebuildShelterCache(currentMap);
            }

            StormShelterManager.FirstTickOfHighstorm = false;
        }

        if (!GenTicks.IsTickInterval(tickInterval)) return;
        scaledCurve = stormIntensityCurve.Evaluate(Mathf.Clamp01(TicksPassed / (float)Duration));

        ProcessItemsInHighstorm();
    }

    private void ProcessItemsInHighstorm() {
        foreach (Verse.Thing thing in SingleMap.listerThings.AllThings
                     .Where(x => x.Map != null && x.ShouldBeMovedByStorm())
                     .ToList()) {
            if (!Rand.Chance(scaledCurve)) continue;

            if (thing.ShouldBeMovedByStorm()) MoveItem(thing);
            if (thing?.Map == null) continue;
            if (thing.Position.Roofed(thing.Map) ||
                !thing.TryGetComp(out InvestitureHolder investiture) ||
                investiture.isFull) {
                continue;
            }

            investiture.AbsorbInvestitureFrom(highstorm, investitureToAbsorb);
        }
    }

    /// <summary>
    ///     Moves items from right to left.
    /// </summary>
    /// @TODO MAJOR overhaul idea: Have a component that keeps track of how much shelter a cell has from any given direction
    private bool MoveItem(Verse.Thing thing) {
        if (!Mod.enableHighstormPushing) return false;
        if (thing.IsBehindSolidThing(IntVec3.East, 2)) return false;
        if (thing is Mineable or Plant) return false;
        if (!thing.CanBeMoved() && !thing.def.useHitPoints) return false;

        IntVec3 oldPos = thing.Position;
        IntVec3 newPos = oldPos + GetRandomStormOffset(thing is Pawn);
        Map map = thing.Map;


        if (!CanMoveToNewPosition(thing, newPos)) {
            if (!newPos.InBounds(map)) return false;

            foreach (Verse.Thing collidedThing in newPos.GetThingList(thing.Map).ToList()) {
                DamageItem(collidedThing);
            }

            DamageItem(thing);
            return false;
        }

        if (thing.def.destroyOnDrop) {
            DamageItem(thing);
        }

        if (newPos.DistanceToEdge(map) == 0) {
            if (thing is not Pawn) {
                thing.DeSpawn();
                return true;
            }
        }

        if (!newPos.InBounds(map)) return true;

        if (thing is not Pawn) {
            if (thing.Spawned) {
                thing.DeSpawn();
            }

            if (!GenPlace.TryPlaceThing(thing, newPos, map, ThingPlaceMode.Near, out Verse.Thing newThing)) {
                GenSpawn.Spawn(thing, oldPos, map);
                return false;
            }

            foreach (Verse.Thing t in newPos.GetThingList(map).ToList()) {
                if (t == newThing) continue;
                if (newThing.CanStackWith(t) && t.TryAbsorbStack(newThing, true)) return true;
                if (t.def.saveCompressible && newThing.def.saveCompressible) {
                    if (MoveItem(t)) return true;

                    newThing.DeSpawn();
                    GenSpawn.Spawn(newThing, oldPos, map);
                    return false;
                }
            }
        } else {
            thing.Position = newPos;
        }

        FleckMaker.ThrowDustPuff(newPos, map, 1.5f);

        return true;
    }

    private bool CanMoveToNewPosition(Verse.Thing thing, IntVec3 newPos) {
        if (!thing.CanBeMoved()) return false;
        if (!newPos.InBounds(thing.Map)) return true;

        return !newPos.GetThingList(thing.Map).Any(t => t.IsSolid());
    }

    private void DamageItem(Verse.Thing thing) {
        if (!Mod.enableHighstormDamage) return;

        Map? map = thing.Map;
        DamageInfo damage = new DamageInfo(
            DamageDefOf.TornadoScratch,
            Rand.Range(0f, 2f),
            instigator: highstorm,
            spawnFilth: false
        );
        switch (thing) {
            case Plant { Destroyed: false } plant when plant.def.plant is { harvestedThingDef: not null }: {
                if (!Rand.Chance(1 / 10f)) {
                    return; // Add ANOTHER gate here. 1/100 chance to destroy, over a BUNCH of ticks, this will still happen, just slower
                }

                // Harvest yield
                Verse.Thing yield = ThingMaker.MakeThing(plant.def.plant.harvestedThingDef);
                yield.stackCount = (int)plant.def.plant.harvestYield;


                // Do damage to the plant
                if (plant.HitPoints - damage.Amount <= 0) {
                    IntVec3 position = plant.Position;
                    plant.Kill(damage);
                    GenPlace.TryPlaceThing(yield, position, map, ThingPlaceMode.Near);
                } else {
                    plant.TakeDamage(damage);
                }

                break;
            }
            case Building building: {
                if (building.Stuff?.stuffProps?.categories?.Contains(StuffCategoryDefOf.Woody) != true) {
                    break;
                }

                building.TakeDamage(damage);
                if (building.Destroyed) {
                    StormShelterManager.RebuildShelterCache(map);
                }

                break;
            }
            case Pawn pawn: {
                if (pawn.Dead) break;
                Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
                if (surgebinder != null) {
                    damage.SetAmount(damage.Amount / (surgebinder.currentIdeal + 1));
                }

                pawn.TakeDamage(damage);

                break;
            }
            default: {
                if (thing.Destroyed) break;

                thing.TakeDamage(damage);

                break;
            }
        }
    }
}