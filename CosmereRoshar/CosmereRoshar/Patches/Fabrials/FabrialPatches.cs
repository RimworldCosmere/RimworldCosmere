using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CosmereRoshar.Comp.Fabrials;
using CosmereRoshar.Comp.Thing;
using CosmereRoshar.Thing.Building;
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

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static void Postfix(Plant __instance, ref float __result) {
        if (__instance.Spawned && IsNearLifeSprenBuilding(__instance) == 1) {
            bool resting = true;
            if (!(GenLocalDate.DayPercent(__instance) < 0.25f)) {
                resting = GenLocalDate.DayPercent(__instance) > 0.8f;
            }

            if (__instance.LifeStage != PlantLifeStage.Growing || resting) {
                __result *= 1f;
                return;
            }

            __result *= 1.25f;
            return;
        }

        if (__instance.Spawned && IsNearLifeSprenBuilding(__instance) == 2) {
            __result *= 0.25f;
        }
    }

    private static int IsNearLifeSprenBuilding(Plant plant) {
        Map? map = plant.Map;
        if (map == null) return 0;
        IntVec3 plantPos = plant.Position;

        foreach (Building? thing in ActiveLifeSprenBuildings) {
            if (thing is FabrialBasicAugmenter building &&
                plantPos.DistanceTo(building.Position) <= 5f) {
                BasicFabrialAugmenter? comp = building.GetComp<BasicFabrialAugmenter>();
                if (comp is { powerOn: true, currentSpren: Spren.Life }) {
                    return 1;
                }
            } else if (thing is FabrialBasicDiminisher diminisher &&
                       plantPos.DistanceTo(diminisher.Position) <= 5f) {
                BasicFabrialDiminisher? comp = diminisher.GetComp<BasicFabrialDiminisher>();
                if (comp is { powerOn: true, currentSpren: Spren.Life }) {
                    return 2;
                }
            }
        }

        return 0;
    }
}