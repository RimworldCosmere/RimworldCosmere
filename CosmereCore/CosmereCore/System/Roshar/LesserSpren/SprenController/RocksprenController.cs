using Cosmere.Core;
using Cosmere.Core.Def;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using UnityEngine;
using Verse;
using ShaderDatabase = Cosmere.System.Roshar.Shader.ShaderDatabase;

namespace Cosmere.System.Roshar.LesserSpren.SprenController;

public class RocksprenController : StaticSprenController {
    public override SprenType SprenType => SprenType.Rockspren;

    public override bool IsEnabled => true;

    public override float CellSpawnChance => 0.05f;

    public override int MinParticlesPerCell => 2;

    public override int MaxParticlesPerCell => 3;

    protected override float MaxSpreadDistance => 0.3f;

    protected override float MovementSpeed => 2f;

    protected override float RandomDirectionAmount => 0.1f;

    public override float SprenSizeMultiplier => 1.5f;

    public override List<GemDef> CompatibleGemTypes => [
        GemDefOf.Garnet,
        GemDefOf.Diamond,
        GemDefOf.Topaz,
    ];

    public override float CaptureRarityMultiplier => 1.0f;

    public override Color SprenColor => new Color(0.6f, 0.5f, 0.4f, 0.8f);

    protected override Material GetBaseMaterial() {
        return ShaderDatabase.CrystallineFacetMaterial;
    }

    protected override void ConfigureMaterial(Material material) {
        base.ConfigureMaterial(material);
        material.SetColor("_Color", SprenColor);
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
                return DefaultSpawnInformation.With(map, position);
            }
        }

        bool thingChecks = map!.thingGrid.ThingsListAt(position)
            .Select(thing => thing.def.defName)
            .Any(defName => defName.StartsWith("Chunk") || defName.Equals("Filth_RubbleRock"));

        return thingChecks ? DefaultSpawnInformation.With(map, position) : null;
    }
}
