using System;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Utility;

public static class StormlightUtilities {
    public static float Normalize(float value, float vMin, float vMax, float tMin, float tMax) {
        return (value - vMin) / (vMax - vMin) * (tMax - tMin) + tMin;
    }

    public static bool IsThingCutGemstone(Verse.Thing thing) {
        return thing.def.Equals(Core.ThingDefOf.CutGem);
    }

    public static bool IsHighstormImmune(Verse.Pawn pawn) {
        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return false;

        if (surgebinder.radiantOrderDef.defName == "Windrunner" && surgebinder.currentIdeal >= 1) return true;
        if (surgebinder.radiantOrderDef.defName == "Bondsmith" && surgebinder.godsprenName == "Stormfather") return true;

        return false;
    }
}

public static class StormShelterManager {
    private static readonly Dictionary<IntVec3, int> CellToRegionIndex
        = new Dictionary<IntVec3, int>();

    public static bool FirstTickOfHighstorm = true;

    private static readonly List<RegionInfo> Regions
        = [];


    public static void RebuildShelterCache(Map map) {
        CellToRegionIndex.Clear();
        Regions.Clear();

        foreach (IntVec3 cell in map.AllCells) {
            if (!cell.Roofed(map)) continue;
            if (CellToRegionIndex.ContainsKey(cell)) continue;

            Building b = cell.GetEdifice(map);
            if (IsWallOrClosedDoor(b)) continue;

            HashSet<IntVec3> newRegionCells = FloodFillRoofedArea(cell, map);

            bool isShelter = IsCShelter(newRegionCells, map);
            RegionInfo newRegion = new RegionInfo {
                cells = newRegionCells,
                isShelter = isShelter,
            };
            Regions.Add(newRegion);

            int regionIndex = Regions.Count - 1;
            foreach (IntVec3 c in newRegionCells) {
                CellToRegionIndex[c] = regionIndex;
            }
        }
    }

    public static bool IsInsideShelter(IntVec3 pos) {
        if (!CellToRegionIndex.TryGetValue(pos, out int idx)) return false;
        return Regions[idx].isShelter;
    }

    public static bool IsProtectedByShelter(IntVec3 pos, Map map) {
        if (IsInsideShelter(pos)) return true;
        for (int i = 0; i < GenAdj.CardinalDirections.Length; i++) {
            IntVec3 adj = pos + GenAdj.CardinalDirections[i];
            if (!adj.InBounds(map)) continue;
            if (IsInsideShelter(adj)) return true;
        }
        return false;
    }

    public static IntVec3 FindNearestShelterCell(IntVec3 from, Map map, TraverseParms traverseParms) {
        if (IsInsideShelter(from)) return from;

        if (Regions.Count == 0) RebuildShelterCache(map);

        int cellCount = GenRadial.NumCellsInRadius(Math.Min(120f, GenRadial.MaxRadialPatternRadius - 1f));
        for (int i = 0; i < cellCount; i++) {
            IntVec3 cell = from + GenRadial.RadialPattern[i];
            if (!cell.InBounds(map)) continue;
            if (!IsInsideShelter(cell)) continue;
            if (!cell.Standable(map)) continue;
            if (!map.reachability.CanReach(from, cell, PathEndMode.OnCell, traverseParms)) continue;
            return cell;
        }

        return IntVec3.Invalid;
    }

    public static void ClearCache() {
        CellToRegionIndex.Clear();
        Regions.Clear();
    }

    private static HashSet<IntVec3> FloodFillRoofedArea(IntVec3 start, Map map) {
        HashSet<IntVec3> visited = [];
        Queue<IntVec3> queue = new Queue<IntVec3>();

        if (!start.InBounds(map) || !start.Roofed(map)) return visited;
        if (IsWallOrClosedDoor(start.GetEdifice(map))) return visited;

        visited.Add(start);
        queue.Enqueue(start);

        while (queue.Count > 0) {
            IntVec3 current = queue.Dequeue();

            foreach (IntVec3 dir in GenAdj.CardinalDirections) {
                IntVec3 next = current + dir;

                if (!next.InBounds(map)) continue;
                if (visited.Contains(next)) continue;
                if (!next.Roofed(map)) continue;

                Building b = next.GetEdifice(map);
                if (IsWallOrClosedDoor(b)) continue;

                visited.Add(next);
                queue.Enqueue(next);
            }
        }

        return visited;
    }

    private static bool IsWallOrClosedDoor(Building b) {
        if (b == null) return false;
        if (b.def.passability == Traversability.Impassable) return true;
        if (b.def.IsDoor && b is Building_Door door && !door.Open) return true;
        return false;
    }

    private static bool IsCShelter(HashSet<IntVec3> area, Map map) {
        if (area == null || area.Count == 0) return false;

        int maxX = int.MinValue;
        foreach (IntVec3 p in area) {
            if (p.x > maxX) maxX = p.x;
        }

        foreach (IntVec3 pos in area) {
            if (pos.x != maxX) continue;

            IntVec3 east = pos + IntVec3.East;
            if (!east.InBounds(map)) continue;

            Building eastBuilding = east.GetEdifice(map);
            if (eastBuilding == null) return false;
            if (eastBuilding.def.passability == Traversability.Impassable) continue;
            if (eastBuilding.def.IsDoor) continue;
            return false;
        }

        return true;
    }

    private struct RegionInfo {
        public HashSet<IntVec3> cells;
        public bool isShelter;
    }
}
