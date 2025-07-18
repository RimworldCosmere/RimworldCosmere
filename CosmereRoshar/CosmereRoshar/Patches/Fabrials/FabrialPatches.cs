using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereRoshar;

[HarmonyPatch(typeof(Plant), "get_GrowthRate")]
public static class CultivationSprenPatch {
    private static readonly List<Building> activeLifeSprenBuildings = new List<Building>();


    public static void RegisterBuilding(Building building) {
        if (building.GetComp<CompBasicFabrialAugumenter>()?.CurrentSpren == Spren.Life) {
            activeLifeSprenBuildings.Add(building);
        } else if (building.GetComp<CompBasicFabrialDiminisher>()?.CurrentSpren == Spren.Life) {
            activeLifeSprenBuildings.Add(building);
        }
    }

    public static void UnregisterBuilding(Building building) {
        activeLifeSprenBuildings.Remove(building);
    }

    public static void Postfix(Plant __instance, ref float __result) {
        if (__instance.Spawned && IsNearLifeSprenBuilding(__instance) == 1) {
            bool resting = true;
            if (!(GenLocalDate.DayPercent(__instance) < 0.25f)) {
                resting = GenLocalDate.DayPercent(__instance) > 0.8f;
            }

            if (__instance.LifeStage != PlantLifeStage.Growing || resting) {
                __result *= 1f;
            } else {
                __result *= 1.25f;
            }
        } else if (__instance.Spawned && IsNearLifeSprenBuilding(__instance) == 2) {
            __result *= 0.25f;
        }
    }

    private static int IsNearLifeSprenBuilding(Plant plant) {
        Map? map = plant.Map;
        if (map == null) return 0;
        IntVec3 plantPos = plant.Position;

        foreach (Building? thing in activeLifeSprenBuildings) {
            if (thing is Building_Fabrial_Basic_Augmenter building &&
                plantPos.DistanceTo(building.Position) <= 5f) {
                CompBasicFabrialAugumenter? comp = building.GetComp<CompBasicFabrialAugumenter>();
                if (comp != null && comp.PowerOn && comp.CurrentSpren == Spren.Life) {
                    return 1;
                }
            } else if (thing is Building_Fabrial_Basic_Diminisher diminisher &&
                       plantPos.DistanceTo(diminisher.Position) <= 5f) {
                CompBasicFabrialDiminisher? comp = diminisher.GetComp<CompBasicFabrialDiminisher>();
                if (comp != null && comp.PowerOn && comp.CurrentSpren == Spren.Life) {
                    return 2;
                }
            }
        }

        return 0;
    }
}