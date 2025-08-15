using Verse;

namespace Cosmere.Roshar.ParticleSystem.LesserSpren.SprenControllers;

public class WavesprenController : BaseSprenController {
    public override SprenType sprenType => SprenType.Wavespren;
    public override bool isEnabled => true;
    public override bool isNatureSpren => true;

    public override bool IsCellValid(IntVec3 position, Map map) {
        if (!IsInBounds(position, map)) return false;

        TerrainDef? terrain = GetTerrain(position, map);
        if (terrain == null) return false;
        if (terrain.waterBodyType.Equals(WaterBodyType.None)) return false;

        // Don't spawn on rivers
        if (HasTerrainTag(terrain, "River")) return false;

        // Only spawn on shorelines - water cells that are adjacent to land
        foreach (IntVec3 adjacentCell in GenAdj.CellsAdjacent8Way(new TargetInfo(position, map))) {
            if (!IsInBounds(adjacentCell, map)) continue;

            TerrainDef? adjacentTerrain = GetTerrain(adjacentCell, map);
            if (adjacentTerrain is { waterBodyType: WaterBodyType.None }) {
                // Found an adjacent land cell, this is a shoreline
                return true;
            }
        }

        // No adjacent land cells found, not a shoreline
        return false;
    }
}