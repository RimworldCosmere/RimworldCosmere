using Cosmere.Core;
using Cosmere.Core.Def;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using RimWorld;
using UnityEngine;
using Verse;
using ShaderDatabase = Cosmere.System.Roshar.Shader.ShaderDatabase;

namespace Cosmere.System.Roshar.LesserSpren.SprenController;

public class WindsprenController : StaticSprenController {
    public override SprenType sprenType => SprenType.Windspren;
    public override bool isEnabled => true;
    public override float cellSpawnChance => 0.02f;
    public override int minParticlesPerCell => 2;
    public override int maxParticlesPerCell => 4;
    protected override float maxSpreadDistance => 0.3f;
    protected override float movementSpeed => 2f;
    protected override float randomDirectionAmount => 0.1f;

    public override List<GemDef> compatibleGemTypes => [
        GemDefOf.Emerald,
        GemDefOf.Diamond,
        GemDefOf.Heliodor,
    ];

    public override float captureRarityMultiplier => 1.0f;
    public override Color sprenColor => new Color(0.7f, 0.7f, 0.9f, 1.0f);

    protected override Material GetBaseMaterial() {
        return ShaderDatabase.FlowingParticleStreamMaterial;
    }

    protected override void ConfigureMaterial(Material material) {
        base.ConfigureMaterial(material);
        material.SetColor("_Color", sprenColor);
        material.SetColor("_StreamColor", sprenColor);
        material.SetColor("_ParticleColor", new Color(1f, 1f, 1f, 0.8f));
        material.SetInt("_FlowPattern", 1);
        material.SetFloat("_FlowSpeed", 2.0f);
        material.SetFloat("_FlowDensity", 12f);
    }

    public override SprenSpawnInformation? GetSprenSpawnInformation(
        IntVec3 position,
        Map? map
    ) {
        if (!IsInBounds(position, map)) return null;

        float windTurbineMultiplier = GetWindTurbineMultiplier(position, map!);

        if (windTurbineMultiplier > 0) {
            return defaultSpawnInformation.With(
                map,
                position,
                cellSpawnChance * windTurbineMultiplier,
                Mathf.RoundToInt(minParticlesPerCell * windTurbineMultiplier),
                Mathf.RoundToInt(maxParticlesPerCell * windTurbineMultiplier)
            );
        }

        TerrainDef? terrain = GetTerrain(position, map!);
        if (terrain == null) return null;

        bool isOpenTerrain = HasTerrainTag(terrain, "Road") ||
                             HasTerrainTag(terrain, "Bridge") ||
                             TerrainNameContains(terrain, "bridge", "road", "path", "paved") ||
                             IsElevatedTerrain(terrain);

        if (!isOpenTerrain) return null;

        bool isClearArea = IsClearOfObstacles(position, map!);

        return isClearArea ? defaultSpawnInformation.With(map, position) : null;
    }

    private static bool IsElevatedTerrain(TerrainDef terrain) {
        return terrain.pathCost <= 2 &&
               !HasTerrainTag(terrain, "Water") &&
               !TerrainNameContains(terrain, "mud", "marsh", "swamp", "bog");
    }

    private static bool IsClearOfObstacles(IntVec3 position, Map map) {
        foreach (IntVec3 checkCell in GenRadial.RadialCellsAround(position, 1, true)) {
            if (!IsInBounds(checkCell, map)) continue;

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

        foreach (IntVec3 checkCell in GenRadial.RadialCellsAround(position, 7, false)) {
            if (!IsInBounds(checkCell, map)) continue;

            foreach (Verse.Thing thing in map.thingGrid.ThingsListAt(checkCell)) {
                CompPowerPlantWind? windComp = thing.TryGetComp<CompPowerPlantWind>();
                if (windComp == null) continue;

                IntVec3 turbineToPos = position - checkCell;
                float distance = turbineToPos.LengthHorizontal;
                if (distance > 5f) continue;

                Rot4 turbineRotation = thing.Rotation;
                IntVec3 turbineFacing = turbineRotation.FacingCell;

                Vector3 turbineToVec = new Vector3(turbineToPos.x, 0, turbineToPos.z).normalized;
                Vector3 facingVec = new Vector3(turbineFacing.x, 0, turbineFacing.z).normalized;

                float dotProduct = Vector3.Dot(turbineToVec, facingVec);
                bool isAligned = Mathf.Abs(dotProduct) > 0.7f;

                if (!isAligned) continue;

                turbinesFound++;

                float windSpeed = map.windManager.WindSpeed;
                float normalizedWind = windSpeed / 3.0f;
                normalizedWind = Mathf.Clamp01(normalizedWind);
                float distanceFalloff = 1.0f - distance / 5.0f;
                float multiplier = 1.0f + normalizedWind * 16.0f * distanceFalloff;

                if (multiplier > maxMultiplier) {
                    maxMultiplier = multiplier;
                }
            }
        }

        return maxMultiplier;
    }
}