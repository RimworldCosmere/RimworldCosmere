using Cosmere.Resources;
using Cosmere.Resources.Def;
using Cosmere.Roshar.LesserSpren.ParticleSystem;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.LesserSpren.SprenControllers;

public class WavesprenController : StaticSprenController {
    public override SprenType sprenType => SprenType.Wavespren;
    public override bool isEnabled => true;
    public override float cellSpawnChance => 0.05f;
    public override int minParticlesPerCell => 1;
    public override int maxParticlesPerCell => 2;
    protected override float maxSpreadDistance => 0.3f;
    protected override float movementSpeed => 2f;
    protected override float randomDirectionAmount => 0.1f;

    public override List<GemDef> compatibleGemTypes => [
        GemDefOf.Sapphire,
        GemDefOf.Diamond,
        GemDefOf.Topaz,
    ];

    public override float captureRarityMultiplier => 1.0f;
    public override Color sprenColor => new Color(0.3f, 0.6f, 0.9f, 0.8f); // Blue water color

    public override SprenSpawnInformation? GetSprenSpawnInformation(
        IntVec3 position,
        Map? map,
        bool isDynamicCell = false
    ) {
        if (!IsInBounds(position, map)) return null;

        // Check if this is a water tile adjacent to land (shore)
        TerrainDef? terrain = GetTerrain(position, map);
        if (terrain == null) return null;

        // Must be water terrain
        bool isWater = HasTerrainTag(terrain, "Water") ||
                       TerrainNameContains(terrain, "water", "ocean", "sea", "lake", "river", "marsh", "swamp");

        if (!isWater) return null;

        // Check if adjacent to land (shore detection)
        bool isShore = IsAdjacentToLand(position, map);

        return isShore ? defaultSpawnInformation.With(map, position) : null;
    }

    private static bool IsAdjacentToLand(IntVec3 position, Map map) {
        // Check all 8 adjacent cells for land terrain
        foreach (IntVec3 adjCell in GenAdj.CellsAdjacent8Way(new TargetInfo(position, map))) {
            if (!IsInBounds(adjCell, map)) continue;

            TerrainDef? adjTerrain = GetTerrain(adjCell, map);
            if (adjTerrain == null) continue;

            // If adjacent cell is NOT water, it's land
            bool isAdjWater = HasTerrainTag(adjTerrain, "Water") ||
                              TerrainNameContains(
                                  adjTerrain,
                                  "water",
                                  "ocean",
                                  "sea",
                                  "lake",
                                  "river",
                                  "marsh",
                                  "swamp"
                              );

            if (!isAdjWater) {
                return true; // Found adjacent land tile
            }
        }

        return false; // No adjacent land found
    }
}