using Cosmere;
using Cosmere.Def;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.LesserSpren.SprenController;

public class WindsprenController : StaticSprenController {
    public override SprenType sprenType => SprenType.Windspren;
    public override bool isEnabled => true;
    public override float cellSpawnChance => 0.02f; // Increased from 0.005f to 0.02f (2%)
    public override int minParticlesPerCell => 2; // Increased from 1
    public override int maxParticlesPerCell => 4; // Increased from 2
    protected override float maxSpreadDistance => 0.3f;
    protected override float movementSpeed => 2f;
    protected override float randomDirectionAmount => 0.1f;

    public override List<GemDef> compatibleGemTypes => [
        GemDefOf.Emerald,
        GemDefOf.Diamond,
        GemDefOf.Heliodor,
    ];

    public override float captureRarityMultiplier => 1.0f;
    public override Color sprenColor => new Color(0.7f, 0.7f, 0.9f, 1.0f); // More saturated blue, full alpha

    protected override Material GetBaseMaterial() {
        return ShaderDatabase.FlowingParticleStreamMaterial;
    }

    protected override void ConfigureMaterial(Material material) {
        base.ConfigureMaterial(material);
        material.SetColor("_Color", sprenColor); // Set the blue color
        material.SetColor("_StreamColor", sprenColor); // Set stream color too
        material.SetColor("_ParticleColor", new Color(1f, 1f, 1f, 0.8f)); // White particles for contrast
        material.SetInt("_FlowPattern", 1); // Wind pattern
        material.SetFloat("_FlowSpeed", 2.0f);
        material.SetFloat("_FlowDensity", 12f); // Much higher density
    }

    public override SprenSpawnInformation? GetSprenSpawnInformation(
        IntVec3 position,
        Map? map
    ) {
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
        int turbinesFound = 0;

        // Check within larger radius for wind turbines
        foreach (IntVec3 checkCell in GenRadial.RadialCellsAround(position, 7, false)) {
            if (!IsInBounds(checkCell, map)) continue;

            foreach (Verse.Thing thing in map.thingGrid.ThingsListAt(checkCell)) {
                CompPowerPlantWind? windComp = thing.TryGetComp<CompPowerPlantWind>();
                if (windComp == null) continue;

                // Calculate direction from turbine to position
                IntVec3 turbineToPos = position - checkCell;
                float distance = turbineToPos.LengthHorizontal;
                if (distance > 5f) continue;

                // Get turbine's facing direction
                Rot4 turbineRotation = thing.Rotation;
                IntVec3 turbineFacing = turbineRotation.FacingCell;

                // Check if position is aligned with turbine's facing direction (front or back)
                // Normalize both vectors manually for dot product
                Vector3 turbineToVec = new Vector3(turbineToPos.x, 0, turbineToPos.z).normalized;
                Vector3 facingVec = new Vector3(turbineFacing.x, 0, turbineFacing.z).normalized;

                float dotProduct = Vector3.Dot(turbineToVec, facingVec);
                bool isAligned = Mathf.Abs(dotProduct) > 0.7f; // Allow some tolerance

                if (!isAligned) continue;

                turbinesFound++;

                // Use the wind speed directly from the map
                float windSpeed = map.windManager.WindSpeed;

                Log.Message(
                    $"Found wind turbine at {checkCell}, facing: {turbineFacing}, wind speed: {windSpeed:F2}, distance: {distance:F1}, aligned: {isAligned}"
                );

                // Convert to multiplier with distance falloff
                float normalizedWind = windSpeed / 3.0f;
                normalizedWind = Mathf.Clamp01(normalizedWind);
                float distanceFalloff = 1.0f - distance / 5.0f; // Linear falloff over 5 cells
                float multiplier = 1.0f + normalizedWind * 16.0f * distanceFalloff;

                if (multiplier > maxMultiplier) {
                    maxMultiplier = multiplier;
                }

                Log.Message($"Wind multiplier: {multiplier:F2} (distance falloff: {distanceFalloff:F2})");
            }
        }

        if (turbinesFound > 0) {
            Log.Message(
                $"Position {position}: Found {turbinesFound} aligned turbines, max multiplier: {maxMultiplier:F2}"
            );
        }

        return maxMultiplier;
    }
}