using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Random = System.Random;

namespace CosmereRoshar;

public static class StormlightUtilities {
    private static readonly List<ThingDef> gems = new List<ThingDef> {
        CosmereRosharDefs.whtwl_RawDiamond,
        CosmereRosharDefs.whtwl_RawGarnet,
        CosmereRosharDefs.whtwl_RawRuby,
        CosmereRosharDefs.whtwl_RawSapphire,
        CosmereRosharDefs.whtwl_RawEmerald,
    };

    private static readonly Random rng = new Random();

    public static void SpeakOaths(
        Pawn pawn,
        PawnStats pawnStats,
        TraitDef traitDef,
        string mainText,
        string titleText,
        string acceptText = "Speak the words",
        string declineText = "Say nothing"
    ) {
        Find.WindowStack.Add(
            new Dialog_MessageBox(
                mainText,
                acceptText,
                () => {
                    pawn.story.traits.GainTrait(new Trait(traitDef));
                    pawnStats.hasFormedBond = true;
                    pawnStats.requirementMap[traitDef.defName][pawnStats.Props.Req_0_1].IsSatisfied = true;
                },
                declineText,
                () => { },
                titleText
            )
        );
    }


    public static bool PawnHasAbility(Pawn pawn, AbilityDef def) {
        return pawn.abilities?.abilities?.Any(ab => ab.def == def) == true;
    }

    public static bool IsRadiant(Trait trait) {
        return trait.def == CosmereRosharDefs.whtwl_Radiant_Windrunner ||
               trait.def == CosmereRosharDefs.whtwl_Radiant_Truthwatcher ||
               trait.def == CosmereRosharDefs.whtwl_Radiant_Edgedancer ||
               trait.def == CosmereRosharDefs.whtwl_Radiant_Skybreaker;
    }

    public static bool IsRadiant(Pawn pawn) {
        if (pawn.NonHumanlikeOrWildMan()) return false;
        if (pawn.AnimalOrWildMan()) return false;
        if (pawn == null) return false;

        Trait trait =
            pawn.story.traits.allTraits.FirstOrDefault(t => CosmereRosharUtilities.RadiantTraits.Contains(t.def));
        if (trait != null) {
            return trait.def == CosmereRosharDefs.whtwl_Radiant_Windrunner ||
                   trait.def == CosmereRosharDefs.whtwl_Radiant_Truthwatcher ||
                   trait.def == CosmereRosharDefs.whtwl_Radiant_Edgedancer ||
                   trait.def == CosmereRosharDefs.whtwl_Radiant_Skybreaker;
        }

        return false;
    }

    public static int GetGraphicId(Thing thing) {
        if (thing == null) return 0;
        CompShardblade comp = thing.TryGetComp<CompShardblade>();
        if (comp == null) return 0;
        return comp.graphicId;
    }

    public static float Normalize(float value, float v_min, float v_max, float t_min, float t_max) {
        return (value - v_min) / (v_max - v_min) * (t_max - t_min) + t_min;
    }

    public static float SprenBaseCaptureProbability(float currentStormlight, float minStormlight, float maxStormlight) {
        float x = Normalize(currentStormlight, minStormlight, maxStormlight, 0f, 100f);
        const float a = 1.1585f;
        const float b = 1690.6f;
        const float c = 100f;
        const float d = 1f;

        return -(a / b) * x * (x - c) * (x - d);
    }

    public static bool IsAnyFireNearby(Building building, float radius = 5f) {
        IntVec3 position = building.Position;
        Map map = building.Map;

        foreach (IntVec3 cell in GenRadial.RadialCellsAround(position, radius, true)) {
            foreach (Thing thing in cell.GetThingList(map)) {
                if (thing.def == RimWorld.ThingDefOf.Fire ||
                    thing.def.category == ThingCategory.Building &&
                    thing.TryGetComp<CompRefuelable>()?.Props.fuelConsumptionPerTickInRain > 0f) {
                    return true;
                }

                if (thing.def.defName.Contains("Torch") || thing.def.defName.Contains("Campfire")) {
                    return true;
                }
            }
        }

        return false;
    }

    public static int GetNumberOfFiresNearby(Building building, float radius = 5f) {
        IntVec3 position = building.Position;
        Map map = building.Map;
        int numberOfFires = 0;
        foreach (IntVec3 cell in GenRadial.RadialCellsAround(position, radius, true)) {
            foreach (Thing thing in cell.GetThingList(map)) {
                if (thing.def == RimWorld.ThingDefOf.Fire ||
                    thing.def.category == ThingCategory.Building &&
                    thing.TryGetComp<CompRefuelable>()?.Props.fuelConsumptionPerTickInRain > 0f) {
                    numberOfFires++;
                } else if (thing.def.defName.Contains("Torch") || thing.def.defName.Contains("Campfire")) {
                    numberOfFires++;
                }
            }
        }

        return numberOfFires;
    }

    public static float GetAverageSuroundingTemperature(Building building, float radius = 5f) {
        IntVec3 position = building.Position;
        Map map = building.Map;
        List<float> temps = new List<float>();
        IEnumerable<IntVec3>? cells = GenRadial.RadialCellsAround(position, radius, true);
        foreach (IntVec3 cell in cells) {
            temps.Add(cell.GetTemperature(building.Map));
        }

        return temps.Average();
    }

    public static float GetSuroundingPain(Building building, float radius = 5f) {
        IntVec3 position = building.Position;
        Map map = building.Map;
        List<float> pains = new List<float>();
        IEnumerable<IntVec3>? cells = GenRadial.RadialCellsAround(position, radius, true);
        foreach (IntVec3 cell in cells) {
            Pawn pawn = cell.GetFirstPawn(map);
            if (pawn != null) {
                pains.Add(pawn.health.hediffSet.PainTotal);
            }
        }

        return pains.Sum();
    }

    public static float GetSuroundingPlants(Building building, float radius = 5f) {
        IntVec3 position = building.Position;
        Map map = building.Map;
        int plants = 0;
        IEnumerable<IntVec3>? cells = GenRadial.RadialCellsAround(position, radius, true);
        foreach (IntVec3 cell in cells) {
            if (cell.InBounds(map)) {
                foreach (Thing thing in cell.GetThingList(map)) {
                    if (thing is Plant plant) {
                        plants++;
                    }
                }
            }
        }

        return plants;
    }

    public static bool ResearchBeingDoneNearby(Building building, float radius = 5f) {
        IntVec3 position = building.Position;
        Map map = building.Map;
        IEnumerable<IntVec3>? cells = GenRadial.RadialCellsAround(position, radius, true);
        foreach (IntVec3 cell in cells) {
            Pawn pawn = cell.GetFirstPawn(map);
            if (pawn != null && pawn.jobs?.curJob != null && pawn.jobs.curJob.def == JobDefOf.Research) {
                return true;
            }
        }

        return false;
    }

    public static bool isThingCutGemstone(Thing thing) {
        return thing.def == CosmereRosharDefs.whtwl_CutRuby ||
               thing.def == CosmereRosharDefs.whtwl_CutEmerald ||
               thing.def == CosmereRosharDefs.whtwl_CutDiamond ||
               thing.def == CosmereRosharDefs.whtwl_CutSapphire ||
               thing.def == CosmereRosharDefs.whtwl_CutGarnet;
    }

    public static Trait GetRadiantTrait(Pawn pawn) {
        return pawn.story.traits.allTraits.FirstOrDefault(t => CosmereRosharUtilities.RadiantTraits.Contains(t.def));
    }

    public static ThingDef RollForRandomGemSpawn() {
        foreach (ThingDef? gem in gems) {
            if (RollTheDice(0, gem.GetCompProperties<CompProperties_RawGemstone>().spawnChance, 1)) {
                return gem;
            }
        }

        return null;
    }

    public static bool RollTheDice(int min, int max, int lowerThreshold) {
        return rng.Next(min, max) <= lowerThreshold;
    }

    public static int RollTheDice(int min, int max) {
        return rng.Next(min, max);
    }

    public static string RollForRandomString(List<string> stringList) {
        int i = rng.Next(stringList.Count);
        return stringList[i];
    }

    public static int RollForRandomIntFromList(List<int> intList) {
        int i = rng.Next(intList.Count);
        return intList[i];
    }


    public static void SetThingGraphic(Thing thing, string texPath, float drawSize = 1.5f) {
        if (thing == null || thing.def == null) return;

        Graphic newGraphic = GraphicDatabase.Get<Graphic_Single>(
            texPath,
            ShaderDatabase.Cutout,
            new Vector2(drawSize, drawSize),
            Color.white
        );

        // Set internal field directly
        AccessTools.Field(typeof(Thing), "graphicInt").SetValue(thing, newGraphic);

        // Force refresh
        thing.Notify_ColorChanged();
    }


    public static bool IsNearGrowingPlants(Pawn pawn, float radius = 3f) {
        IntVec3 position = pawn.Position;
        Map map = pawn.Map;
        IEnumerable<IntVec3>? cells = GenRadial.RadialCellsAround(position, radius, true);

        foreach (IntVec3 cell in cells) {
            if (cell.InBounds(map)) {
                foreach (Thing thing in cell.GetThingList(map)) {
                    if (thing is Plant plant && plant.Growth > 0.05f && plant.Growth < 1.0f && plant.GrowthRate > 0f) {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public static bool IsPawnEligibleForDoctoring(Pawn pawn) {
        if (pawn == null || pawn.Dead || pawn.AnimalOrWildMan() || pawn.NonHumanlikeOrWildMan()) {
            return false;
        }

        return !pawn.skills.GetSkill(SkillDefOf.Medicine).TotallyDisabled;
    }

    public static bool ShouldBeMovedByStorm(this Thing thing) {
        if (!thing.Spawned || thing.Map == null) {
            return false;
        }

        Room room = thing.GetRoom();

        if (StormShelterManager.IsInsideShelter(thing.Position)) return false;
        if (room == null) return true;
        if (room.PsychologicallyOutdoors) return true;
        if (!thing.Position.Roofed(thing.Map)) return true;

        return false;
    }
    //bottom//
}

public static class StormShelterManager {
    // For each cell, store which region index it belongs to. 
    // If it doesn't belong to any region, it won't be in this dictionary.
    private static readonly Dictionary<IntVec3, int> cellToRegionIndex
        = new Dictionary<IntVec3, int>();

    public static bool FirstTickOfHighstorm = true;

    private static readonly List<RegionInfo> regions
        = new List<RegionInfo>();


    public static void RebuildShelterCache(Map map) {
        // Clear old data
        cellToRegionIndex.Clear();
        regions.Clear();

        // Loop through each cell in the map
        foreach (IntVec3 cell in map.AllCells) {
            // We only care about roofed cells
            if (!cell.Roofed(map)) {
                continue;
            }

            // If this cell is already assigned to a region, skip
            if (cellToRegionIndex.ContainsKey(cell)) {
                continue;
            }

            Building b = cell.GetEdifice(map);
            if (IsWallOrClosedDoor(b)) {
                continue; // Don't BFS from a wall tile
            }

            // Not visited yet -> BFS to form a new region
            HashSet<IntVec3> newRegionCells = FloodFillRoofedArea(cell, map);

            // Check if this region is a c-shelter
            bool isShelter = IsCShelter(newRegionCells, map);
            // Store the region
            RegionInfo newRegion = new RegionInfo {
                cells = newRegionCells,
                isShelter = isShelter,
            };
            regions.Add(newRegion);

            // For each cell in this region, record its region index
            int regionIndex = regions.Count - 1;
            foreach (IntVec3 c in newRegionCells) {
                cellToRegionIndex[c] = regionIndex;
            }
        }
    }

    public static bool IsInsideShelter(IntVec3 pos) {
        if (!cellToRegionIndex.TryGetValue(pos, out int idx)) {
            // Not in any known roofed region
            return false;
        }

        return regions[idx].isShelter;
    }

    public static void ClearCache() {
        cellToRegionIndex.Clear();
        regions.Clear();
    }

    private static HashSet<IntVec3> FloodFillRoofedArea(IntVec3 start, Map map) {
        HashSet<IntVec3> visited = new HashSet<IntVec3>();
        Queue<IntVec3> queue = new Queue<IntVec3>();

        // 1) Must be in bounds & roofed
        if (!start.InBounds(map) || !start.Roofed(map)) {
            return visited;
        }

        // 2) Check that the start cell is not a wall/closed door
        if (IsWallOrClosedDoor(start.GetEdifice(map))) {
            return visited;
        }

        // Start BFS
        visited.Add(start);
        queue.Enqueue(start);

        while (queue.Count > 0) {
            IntVec3 current = queue.Dequeue();

            // Check the 4 cardinal directions
            foreach (IntVec3 dir in GenAdj.CardinalDirections) {
                IntVec3 next = current + dir;

                // a) Must be in bounds
                if (!next.InBounds(map)) {
                    continue;
                }

                // b) Already visited? skip
                if (visited.Contains(next)) {
                    continue;
                }

                // c) Must be roofed
                if (!next.Roofed(map)) {
                    continue;
                }

                // d) Must NOT be a wall or closed door
                Building b = next.GetEdifice(map);
                if (IsWallOrClosedDoor(b)) {
                    continue;
                }

                // If we get here, 'next' is a valid floor/inside cell
                visited.Add(next);
                queue.Enqueue(next);
            }
        }

        return visited;
    }

    private static bool IsWallOrClosedDoor(Building b) {
        if (b == null) return false;

        // If it's a solid wall
        if (b.def.passability == Traversability.Impassable) {
            return true;
        }

        // If it's a door that's closed
        if (b.def.IsDoor && b is Building_Door door && !door.Open) {
            return true;
        }

        return false;
    }


    private static bool IsCShelter(HashSet<IntVec3> area, Map map) {
        if (area == null || area.Count == 0) {
            return false;
        }

        int maxX = area.Max(p => p.x);
        foreach (IntVec3 pos in area) {
            if (pos.x != maxX) {
                continue;
            }

            IntVec3 east = pos + IntVec3.East;
            if (!east.InBounds(map)) {
                continue;
            }

            Building eastWall = east.GetEdifice(map);
            if (eastWall == null || eastWall.def.passability != Traversability.Impassable) {
                return false;
            }
        }

        return true;
    }

    private struct RegionInfo {
        public HashSet<IntVec3> cells;
        public bool isShelter;
    }
}