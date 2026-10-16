using Cosmere.Core;
using Cosmere.Core.Def;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using UnityEngine;
using Verse;
using ShaderDatabase = Cosmere.System.Roshar.Shader.ShaderDatabase;

namespace Cosmere.System.Roshar.LesserSpren.SprenController;

public class GrasssprenController : StaticSprenController {
    public override SprenType SprenType => SprenType.Grassspren;

    public override bool IsEnabled => true;

    public override float CellSpawnChance => 0.016f;

    public override int MaxParticlesPerCell => 2;

    protected override float MaxSpreadDistance => 0.1f;

    protected override float MovementSpeed => 3f;

    protected override float RandomDirectionAmount => 0.2f;

    public override List<GemDef> CompatibleGemTypes => [
        GemDefOf.Emerald,
        GemDefOf.Heliodor,
    ];

    public override float CaptureRarityMultiplier => 1.2f;

    public override Color SprenColor => new Color(0.2f, 0.8f, 0.3f, 0.9f);

    protected override Material GetBaseMaterial() {
        return ShaderDatabase.FlowingParticleStreamMaterial;
    }

    protected override void ConfigureMaterial(Material material) {
        base.ConfigureMaterial(material);
        material.SetColor("_Color", SprenColor);
        material.SetColor("_StreamColor", new Color(0.4f, 1f, 0.5f, 0.8f));
        material.SetColor("_ParticleColor", new Color(0.9f, 1f, 0.9f, 0.8f));
        material.SetInt("_FlowPattern", 0);
        material.SetFloat("_FlowSpeed", 1.2f);
        material.SetFloat("_FlowDensity", 8f);
        material.SetFloat("_AnimationSpeed", 1.0f);
    }

    public override SprenSpawnInformation? GetSprenSpawnInformation(
        IntVec3 position,
        Map? map
    ) {
        if (!IsInBounds(position, map)) return null;

        TerrainDef? terrain = GetTerrain(position, map!);
        if (terrain != null) {
            bool hasGrassTag = HasTerrainTag(terrain, "Grass");
            bool hasGrassName = TerrainNameContains(terrain, "grass", "meadow", "field", "pasture");
            bool hasFertile = HasTerrainTag(terrain, "Fertile");

            if (hasGrassTag || hasGrassName || hasFertile) {
                return DefaultSpawnInformation.With(map, position);
            }
        }

        bool hasPlants = map!.thingGrid.ThingsListAt(position)
            .Any(thing => thing.def.category == ThingCategory.Plant);

        return hasPlants ? DefaultSpawnInformation.With(map, position) : null;
    }
}
