using System;
using System.Collections.Generic;
using CosmereRoshar.Comp.Apparel;
using CosmereRoshar.Comp.Fabrials;
using CosmereRoshar.Comp.Furniture;
using CosmereRoshar.Comp.Thing;
using RimWorld;
using Verse;

namespace CosmereRoshar.Weather;

public class HighstormExtension : DefModExtension {
    public int stormDuration;
    public string windDirection;
    public float windStrength;
}

public class GameConditionHighstorm : GameCondition {
    private Random mRand = new Random();

    public override void End() {
        base.End();
        StormShelterManager.ClearCache();
        if (SingleMap != null) {
            SingleMap.weatherManager.TransitionTo(WeatherDefOf.FoggyRain);
        }
    }


    public override void GameConditionTick() {
        base.GameConditionTick();
        HighstormExtension? ext = def.GetModExtension<HighstormExtension>();
        if (ext == null) return;
        if (CosmereRoshar.enableHighstormPushing && StormShelterManager.FirstTickOfHighstorm) {
            Map currentMap = Find.CurrentMap;
            if (currentMap != null) {
                StormShelterManager.RebuildShelterCache(currentMap);
            }

            StormShelterManager.FirstTickOfHighstorm = false;
        }

        if (Find.TickManager.TicksGame % 8 == 0) {
            TryToInfuseThings();
            if (CosmereRoshar.settings.enableHighstormPushing) {
                MoveItem();
            }
        }
    }

    public void TryToInfuseThings() {
        List<Thing> things = SingleMap.listerThings.ThingsInGroup(ThingRequestGroup.Everything);
        foreach (Thing thing in things) {
            if (thing.Position.Roofed(thing.Map)) {
                continue;
            }

            if (thing.TryGetComp<Stormlight>() is Stormlight stormlightComp) {
                stormlightComp.InfuseStormlight(5f);
            } else if (thing.def == CosmereRosharDefs.WhtwlApparelSpherePouch && !thing.Position.Roofed(thing.Map)) {
                CompSpherePouch? pouch = thing.TryGetComp<CompSpherePouch>();
                if (pouch != null) {
                    pouch.InfuseStormlight(5f);
                }
            } else if (thing.def == CosmereRosharDefs.WhtwlSphereLampWall && !thing.Position.Roofed(thing.Map)) {
                StormlightLamps? lamp = thing.TryGetComp<StormlightLamps>();
                if (lamp != null) {
                    lamp.InfuseStormlight(5f);
                }
            } else if (thing.def == CosmereRosharDefs.WhtwlApparelFabrialPainrialDiminisher &&
                       !thing.Position.Roofed(thing.Map)) {
                CompApparelFabrialDiminisher? comp = thing.TryGetComp<CompApparelFabrialDiminisher>();
                if (comp != null) {
                    comp.InfuseStormlight(5f);
                }
            }
        }
    }

    public void DestoyIfCollide(Thing item, Map itemMap, IntVec3 newPos) {
        List<Thing> thingsHere = itemMap.thingGrid.ThingsListAtFast(newPos).ListFullCopy();
        foreach (Thing thing in thingsHere) {
            if (thing is Plant plant && plant.def.plant != null && plant.def.plant.harvestedThingDef != null) {
                // Harvest yield
                int woodCount = 5;
                Thing wood = ThingMaker.MakeThing(plant.def.plant.harvestedThingDef);
                wood.stackCount = woodCount;

                // Destroy the plant
                plant.Destroy();

                // Place wood
                GenPlace.TryPlaceThing(wood, newPos, itemMap, ThingPlaceMode.Near);
            } else if (thing is Building building) {
                if (building.def.MadeFromStuff &&
                    building.Stuff?.stuffProps?.categories?.Contains(StuffCategoryDefOf.Woody) == true) {
                    //Log.Message($"Damaging wooden wall: {building.def.defName}");
                    building.TakeDamage(
                        new DamageInfo(
                            DamageDefOf.Blunt,
                            10, // amount
                            0, // armorPenetration
                            -1, // angle
                            item
                        )
                    );
                    if (CosmereRoshar.enableHighstormPushing && building.Destroyed) {
                        StormShelterManager.RebuildShelterCache(itemMap);
                    }
                }
            } else if (thing is Pawn pawn) {
                if (!pawn.Dead) {
                    DamageInfo damage = new DamageInfo(
                        DamageDefOf.Blunt,
                        10, // Damage amount
                        0, // Armor penetration
                        -1, // Angle
                        item
                    );


                    // Apply damage
                    pawn.TakeDamage(damage);
                }
            }
        }
    }


    public void MoveItem() {
        // Copy the list to avoid modifying during enumeration
        List<Thing> items = SingleMap.listerThings
            .ThingsInGroup(ThingRequestGroup.HaulableEver)
            .ListFullCopy();

        foreach (Thing item in items) {
            Map itemMap = item.Map;
            if (itemMap == null) continue;
            //if (item.Position.Roofed(itemMap)) continue;

            if (!item.ShouldBeMovedByStorm()) {
                continue;
            }

            // push logic
            IntVec3 newPos = item.Position + IntVec3.West;
            DestoyIfCollide(item, itemMap, newPos);
            if (!newPos.Walkable(itemMap)) {
                continue;
            }


            if (newPos.DistanceToEdge(itemMap) == 0) {
                item.DeSpawn();
            } else {
                item.Position = newPos;
            }
        }
    }
}