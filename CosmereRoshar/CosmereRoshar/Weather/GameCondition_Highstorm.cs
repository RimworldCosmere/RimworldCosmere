using System;
using System.Collections.Generic;
using System.Linq;
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
        List<Verse.Thing> things = SingleMap.listerThings.ThingsInGroup(ThingRequestGroup.Everything)
            .Where(t => t.Position.Roofed(t.Map))
            .ToList();
        foreach (Verse.Thing thing in things) {
            if (thing.TryGetComp(out Stormlight stormlight)) {
                stormlight.InfuseStormlight(5f);
            } else if (thing.def == CosmereRosharDefs.WhtwlApparelSpherePouch && !thing.Position.Roofed(thing.Map)) {
                thing.TryGetComp<CompSpherePouch>()?.InfuseStormlight(5f);
            } else if (thing.def == CosmereRosharDefs.WhtwlSphereLampWall && !thing.Position.Roofed(thing.Map)) {
                thing.TryGetComp<StormlightLamps>()?.InfuseStormlight(5f);
            } else if (thing.def == CosmereRosharDefs.WhtwlApparelFabrialPainrialDiminisher &&
                       !thing.Position.Roofed(thing.Map)) {
                thing.TryGetComp<FabrialDiminisher>()?.InfuseStormlight(5f);
            }
        }
    }

    public void DestoyIfCollide(Verse.Thing item, Map itemMap, IntVec3 newPos) {
        List<Verse.Thing> thingsHere = itemMap.thingGrid.ThingsListAtFast(newPos).ListFullCopy();
        foreach (Verse.Thing thing in thingsHere) {
            if (thing is Plant plant && plant.def.plant != null && plant.def.plant.harvestedThingDef != null) {
                // Harvest yield
                int woodCount = 5;
                Verse.Thing wood = ThingMaker.MakeThing(plant.def.plant.harvestedThingDef);
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
        List<Verse.Thing> items = SingleMap.listerThings
            .ThingsInGroup(ThingRequestGroup.HaulableEver)
            .ListFullCopy();

        foreach (Verse.Thing item in items) {
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