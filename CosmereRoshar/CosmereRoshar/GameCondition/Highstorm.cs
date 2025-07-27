using Cosmere.Core.Comp.Thing;
using Cosmere.Roshar.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.GameCondition;

public class Highstorm : RimWorld.GameCondition {
    private const float totalInvestitureToAbsorbPerItem = 2000f;

    private static readonly SimpleCurve stormIntensityCurve = [
        new CurvePoint(0f, 0), // Start of storm: 0% intensity
        new CurvePoint(0.4f, .75f), // Mid-storm: 100% intensity
        new CurvePoint(0.5f, 0f), // Mid-storm: 100% intensity
        new CurvePoint(0.6f, .75f), // Mid-storm: 100% intensity
        new CurvePoint(1f, 0),
    ];

    private static readonly List<(IntVec3 offset, float weight)> weightedOffsets =
        new List<(IntVec3 offset, float weight)> {
            (IntVec3.West, 5f),
            (IntVec3.NorthWest, 3f),
            (IntVec3.SouthWest, 3f),
            (IntVec3.North, 1f),
            (IntVec3.South, 1f),
        };

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
            if (thing is not Pawn pawn ||
                thing.Position.Roofed(thing.Map) ||
                !pawn.TryGetComp(out InvestitureHolder investiture) ||
                investiture.isFull) {
                continue;
            }

            investiture.AbsorbInvestitureFrom(highstorm, investitureToAbsorb);
        }
    }

    /// <summary>
    ///     Moves items from right to left.
    /// </summary>
    /// @TODO Maybe it should also check if the pawn is at least behind a wall
    private void MoveItem(Verse.Thing thing) {
        if (!Mod.enableHighstormPushing) return;
        if (thing.IsBehindSolidThing(IntVec3.East, 3)) return;

        IntVec3 newPos = thing.Position + GetRandomStormOffset(thing is Pawn);
        if (thing.IsBehindSolidThing(IntVec3.West, 1) || !thing.CanBeMoved()) {
            // Damage both things, in newPos, and thing
            if (!newPos.InBounds(thing.Map)) return;
            foreach (Verse.Thing collidedThing in newPos.GetThingList(thing.Map).ToList()) {
                DamageItem(collidedThing);
            }

            DamageItem(thing);
            return;
        }

        if (thing.def.destroyOnDrop) {
            DamageItem(thing);
        }

        if (newPos.DistanceToEdge(thing.Map) == 0) {
            if (thing is not Pawn) thing.DeSpawn();
        } else {
            thing.Position = newPos;
        }
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

                IntVec3 position = plant.Position;
                // Do damage to the plant
                if (plant.HitPoints - damage.Amount <= 0) {
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
        }
    }
}