using Cosmere.Core;
using Cosmere.Core.Def;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using UnityEngine;
using Verse;
using ShaderDatabase = Cosmere.System.Roshar.Shader.ShaderDatabase;

namespace Cosmere.System.Roshar.LesserSpren.SprenController;

public class RocksprenController : StaticSprenController {
    public override SprenType sprenType => SprenType.Rockspren;

    public override bool isEnabled => true;

    public override float cellSpawnChance => 0.05f;

    public override int minParticlesPerCell => 2;

    public override int maxParticlesPerCell => 3;

    protected override float maxSpreadDistance => 0.3f;

    protected override float movementSpeed => 2f;

    protected override float randomDirectionAmount => 0.1f;

    public override float sprenSizeMultiplier => 1.5f;

    public override List<GemDef> compatibleGemTypes => [
        GemDefOf.Garnet,
        GemDefOf.Diamond,
        GemDefOf.Topaz,
    ];

    public override float captureRarityMultiplier => 1.0f;

    public override Color sprenColor => new Color(0.6f, 0.5f, 0.4f, 0.8f);

    protected override Material GetBaseMaterial() {
        return ShaderDatabase.CrystallineFacetMaterial;
    }

    protected override void ConfigureMaterial(Material material) {
        base.ConfigureMaterial(material);
        material.SetColor("_Color", sprenColor);
        material.SetFloat("_FacetSize", 6f);
        material.SetFloat("_FacetSharpness", 1.2f);
        material.SetFloat("_PulseSpeed", 0.3f);
        material.SetFloat("_PulseIntensity", 0.2f);
    }

    public override SprenSpawnInformation? GetSprenSpawnInformation(IntVec3 position, Map? map) {
        if (!IsInBounds(position, map)) return null;

        TerrainDef? terrain = GetTerrain(position, map!);
        if (terrain != null) {
            bool hasRockTag = HasTerrainTag(terrain, "Rock");
            bool hasRockName = TerrainNameContains(terrain, "rock", "stone", "granite", "marble", "slate", "rubble");

            if (hasRockTag || hasRockName) {
                return defaultSpawnInformation.With(map, position);
            }
        }

        bool thingChecks = map!.thingGrid.ThingsListAt(position)
            .Select(thing => thing.def.defName)
            .Any(defName => defName.StartsWith("Chunk") || defName.Equals("Filth_RubbleRock"));

        return thingChecks ? defaultSpawnInformation.With(map, position) : null;
    }
}
