using Cosmere.Resources;
using Cosmere.Resources.Def;
using Cosmere.Roshar.LesserSpren.ParticleSystem;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.LesserSpren.SprenControllers;

public class WindsprenController : StaticSprenController {
    public override SprenType sprenType => SprenType.Windspren;
    public override bool isEnabled => true;
    public override float cellSpawnChance => 0.005f;
    public override int minParticlesPerCell => 1;
    public override int maxParticlesPerCell => 2;
    protected override float maxSpreadDistance => 0.3f;
    protected override float movementSpeed => 2f;
    protected override float randomDirectionAmount => 0.1f;

    public override List<GemDef> compatibleGemTypes => [
        GemDefOf.Emerald,
        GemDefOf.Diamond,
        GemDefOf.Heliodor,
    ];

    public override float captureRarityMultiplier => 1.0f;
    public override Color sprenColor => new Color(0.8f, 0.9f, 1.0f, 0.8f); // Light blue/white wind color

    public override SprenSpawnInformation? GetSprenSpawnInformation(
        IntVec3 position,
        Map? map,
        bool isDynamicCell = false
    ) {
        if (position == IntVec3.Invalid || map == null) {
            return defaultSpawnInformation;
        }

        if (!IsInBounds(position, map)) return null;

        // Check for wind turbine proximity first (highest priority)
        float windTurbineMultiplier = GetWindTurbineMultiplier(position, map);

        if (windTurbineMultiplier > 0) {
            // Scale spawn chance and particle count based on wind turbine speed
            return defaultSpawnInformation.With(
                map,
                position,
                cellSpawnChance * windTurbineMultiplier, // Variable spawn chance based on wind speed
                Mathf.RoundToInt(minParticlesPerCell * windTurbineMultiplier), // Scale minimum particles
                Mathf.RoundToInt(maxParticlesPerCell * windTurbineMultiplier) // Scale maximum particles
            );
        }

        // Check if this is an elevated or exposed area where wind would be strong
        TerrainDef? terrain = GetTerrain(position, map);
        if (terrain == null) return null;

        // Wind spren prefer open, elevated areas
        bool isOpenTerrain = HasTerrainTag(terrain, "Road") ||
                             HasTerrainTag(terrain, "Bridge") ||
                             TerrainNameContains(terrain, "bridge", "road", "path", "paved") ||
                             IsElevatedTerrain(terrain);

        if (!isOpenTerrain) return null;

        // Check if area is relatively clear of obstacles (buildings, walls)
        bool isClearArea = IsClearOfObstacles(position, map);

        return isClearArea ? defaultSpawnInformation.With(map, position) : null;
    }

    private static bool IsElevatedTerrain(TerrainDef terrain) {
        return terrain.pathCost <= 2 && // Fast movement suggests open area
               !HasTerrainTag(terrain, "Water") &&
               !TerrainNameContains(terrain, "mud", "marsh", "swamp", "bog");
    }

    private static bool IsClearOfObstacles(IntVec3 position, Map map) {
        // Check if the cell and immediate surroundings are relatively clear
        foreach (IntVec3 checkCell in GenRadial.RadialCellsAround(position, 1, true)) {
            if (!IsInBounds(checkCell, map)) continue;

            // Check for buildings, walls, or dense structures
            foreach (Verse.Thing thing in map.thingGrid.ThingsListAt(checkCell)) {
                if (thing.def.category == ThingCategory.Building &&
                    (thing.def.fillPercent > 0.5f || thing.def.passability == Traversability.Impassable)) {
                    return false;
                }
            }
        }

        return true;
    }

    private static float GetWindTurbineMultiplier(IntVec3 position, Map map) {
        float maxMultiplier = 0f;

        // Check within 2 block radius for wind turbines
        foreach (IntVec3 checkCell in GenRadial.RadialCellsAround(position, 2, true)) {
            if (!IsInBounds(checkCell, map)) continue;

            foreach (Verse.Thing thing in map.thingGrid.ThingsListAt(checkCell)) {
                CompPowerPlantWind? windComp = thing.TryGetComp<CompPowerPlantWind>();
                if (windComp != null) {
                    // Get the current wind speed percentage (0-1)
                    float windSpeed = windComp.PowerOutput / (float)(-(double)windComp.Props.PowerConsumption * 1.5);

                    // Convert to multiplier: 1.0 at 0% wind, up to 5.0 at 100% wind
                    float multiplier = 1.0f + windSpeed * 4.0f;

                    // Keep track of the highest multiplier from nearby turbines
                    if (multiplier > maxMultiplier) {
                        maxMultiplier = multiplier;
                    }
                }
            }
        }

        return maxMultiplier;
    }
}