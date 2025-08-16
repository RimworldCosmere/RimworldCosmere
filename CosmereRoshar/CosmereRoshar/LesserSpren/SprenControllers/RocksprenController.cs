using Cosmere.Resources;
using Cosmere.Resources.Def;
using Cosmere.Roshar.LesserSpren.ParticleSystem;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.LesserSpren.SprenControllers;

public class RocksprenController : BaseSprenController {
    public override SprenType sprenType => SprenType.Rockspren;
    public override bool isEnabled => true;
    public override bool isNatureSpren => true;
    public override float cellSpawnChance => 0.15f; // 15% chance for visibility
    public override int maxParticlesPerCell => 6;

    // Cell management for nature spren (cached) - using base class collections
    public override List<IntVec3> validSpawnCells => validSpawnCellsInt;
    public override List<IntVec3> activeSpawnCells => activeSpawnCellsInt;

    // Capture configuration
    public override List<GemDef> compatibleGemTypes => [
        GemDefOf.Garnet,
        GemDefOf.Diamond,
        GemDefOf.Topaz,
    ];

    public override float captureRarityMultiplier => 1.0f;

    // Visual configuration
    public override Color sprenColor => new Color(0.6f, 0.5f, 0.4f, 0.8f); // Brownish gray

    public override SprenSpawnInformation? GetSprenSpawnInformation(
        IntVec3 position,
        Map map,
        bool isDynamicCell = false
    ) {
        if (!IsInBounds(position, map)) return null;


        // Check terrain for rock (async terrain validation)
        TerrainDef? terrain = GetTerrain(position, map);
        if (terrain != null) {
            // Terrain checks
            bool hasRockTag = HasTerrainTag(terrain, "Rock");
            bool hasRockName = TerrainNameContains(terrain, "rock", "stone", "granite", "marble", "slate", "rubble");

            if (hasRockTag || hasRockName) {
                return defaultSpawnInformation.With(
                    position,
                    cellSpawnChance,
                    minParticlesPerCell,
                    maxParticlesPerCell
                );
            }
        }

        // Check for stone chunks at this position
        bool thingChecks = map.thingGrid.ThingsListAt(position)
            .Select(thing => thing.def.defName)
            .Any(defName => defName.StartsWith("Chunk") || defName.Equals("Filth_RubbleRock"));

        if (thingChecks) {
            return defaultSpawnInformation.With(position, cellSpawnChance, minParticlesPerCell, maxParticlesPerCell);
        }

        return null;
    }

    // Implementation of required abstract methods from base class
    protected override void RefreshValidCells(Map map) {
        validSpawnCellsInt.Clear();

        // Scan all cells and cache valid spawn locations
        foreach (IntVec3 cell in map.AllCells) {
            SprenSpawnInformation? spawnInfo = GetSprenSpawnInformation(cell, map);
            if (spawnInfo != null) {
                validSpawnCellsInt.Add(cell);
            }
        }
    }

    protected override void RefreshActiveCells(Map map) {
        activeSpawnCellsInt.Clear();

        // Only activate cells from the current valid cells
        foreach (IntVec3 cell in validSpawnCellsInt) {
            if (Rand.Chance(cellSpawnChance)) {
                activeSpawnCellsInt.Add(cell);
            }
        }
    }
}