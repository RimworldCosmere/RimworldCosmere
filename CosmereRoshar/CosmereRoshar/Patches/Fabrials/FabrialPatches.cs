using System.Collections.Generic;
using CosmereRoshar.Comp.Fabrials;
using CosmereRoshar.Comp.Thing;
using CosmereRoshar.Thing;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereRoshar.Patches.Fabrials;

[HarmonyPatch(typeof(Plant), "get_GrowthRate")]
public static class CultivationSprenPatch {
    private static readonly List<Building> ActiveLifeSprenBuildings = new List<Building>();


    public static void RegisterBuilding(Building building) {
        if (building.GetComp<BasicFabrialAugmenter>()?.currentSpren == Spren.Life) {
            ActiveLifeSprenBuildings.Add(building);
        } else if (building.GetComp<BasicFabrialDiminisher>()?.currentSpren == Spren.Life) {
            ActiveLifeSprenBuildings.Add(building);
        }
    }

    public static void UnregisterBuilding(Building building) {
        ActiveLifeSprenBuildings.Remove(building);
    }

    public static void Postfix(Plant instance, ref float result) {
        if (instance.Spawned && IsNearLifeSprenBuilding(instance) == 1) {
            bool resting = true;
            if (!(GenLocalDate.DayPercent(instance) < 0.25f)) {
                resting = GenLocalDate.DayPercent(instance) > 0.8f;
            }

            if (instance.LifeStage != PlantLifeStage.Growing || resting) {
                result *= 1f;
            } else {
                result *= 1.25f;
            }
        } else if (instance.Spawned && IsNearLifeSprenBuilding(instance) == 2) {
            result *= 0.25f;
        }
    }

    private static int IsNearLifeSprenBuilding(Plant plant) {
        Map? map = plant.Map;
        if (map == null) return 0;
        IntVec3 plantPos = plant.Position;

        foreach (Building? thing in ActiveLifeSprenBuildings) {
            if (thing is BuildingFabrialBasicAugmenter building &&
                plantPos.DistanceTo(building.Position) <= 5f) {
                BasicFabrialAugmenter? comp = building.GetComp<BasicFabrialAugmenter>();
                if (comp != null && comp.powerOn && comp.currentSpren == Spren.Life) {
                    return 1;
                }
            } else if (thing is BuildingFabrialBasicDiminisher diminisher &&
                       plantPos.DistanceTo(diminisher.Position) <= 5f) {
                BasicFabrialDiminisher? comp = diminisher.GetComp<BasicFabrialDiminisher>();
                if (comp != null && comp.powerOn && comp.currentSpren == Spren.Life) {
                    return 2;
                }
            }
        }

        return 0;
    }
}