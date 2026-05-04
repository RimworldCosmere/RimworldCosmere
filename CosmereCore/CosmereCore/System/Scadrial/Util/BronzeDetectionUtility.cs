using Verse;
using System.Text;
using RimWorld.Planet;

namespace Cosmere.System.Scadrial.Util;

public static class BronzeDetectionUtility {
    public static string? GetTileResourceReport(int tileId) {
        List<ThingDef> rockTypes = Find.World.NaturalRockTypesIn(new PlanetTile(tileId)).ToList();
        if (rockTypes.Count == 0) return null;

        StringBuilder sb = new StringBuilder();
        HashSet<string> reportedMinerals = [];

        for (int i = 0; i < rockTypes.Count; i++) {
            ThingDef rock = rockTypes[i];
            ThingDef? mineable = rock.building?.mineableThing;
            if (mineable == null) continue;

            string mineralLabel = mineable.LabelCap;
            if (reportedMinerals.Add(mineralLabel)) {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(mineralLabel);
            }
        }

        if (sb.Length == 0) return null;

        return sb.ToString();
    }

    public static List<int> GetTilesInRange(int centerTileId, int depth) {
        HashSet<int> visited = [centerTileId];
        List<int> frontier = [centerTileId];
        List<int> result = [];

        for (int ring = 0; ring < depth; ring++) {
            List<int> nextFrontier = [];
            List<PlanetTile> neighbors = [];
            for (int i = 0; i < frontier.Count; i++) {
                neighbors.Clear();
                Find.WorldGrid.GetTileNeighbors(new PlanetTile(frontier[i]), neighbors);
                for (int j = 0; j < neighbors.Count; j++) {
                    int neighborId = neighbors[j].tileId;
                    if (!visited.Add(neighborId)) continue;
                    nextFrontier.Add(neighborId);
                    result.Add(neighborId);
                }
            }

            frontier = nextFrontier;
        }

        return result;
    }
}