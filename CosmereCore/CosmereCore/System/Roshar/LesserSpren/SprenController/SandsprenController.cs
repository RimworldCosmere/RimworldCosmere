using Cosmere.Core;
using Cosmere.Core.Def;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.LesserSpren.SprenController;

public class SandsprenController : StaticSprenController {
    public override SprenType sprenType => SprenType.Sandspren;
    public override bool isEnabled => true;
    public override float cellSpawnChance => 0.025f;
    public override int minParticlesPerCell => 1;
    public override int maxParticlesPerCell => 2;

    public override List<GemDef> compatibleGemTypes => [
        GemDefOf.Garnet,
        GemDefOf.Diamond,
        GemDefOf.Topaz,
    ];

    public override float captureRarityMultiplier => 1.0f;
    public override Color sprenColor => new Color(0.9f, 0.8f, 0.5f, 0.6f);

    public override SprenSpawnInformation? GetSprenSpawnInformation(
        IntVec3 position,
        Map? map
    ) {
        if (!IsInBounds(position, map)) return null;

        TerrainDef? terrain = GetTerrain(position, map!);
        if (terrain != null) {
            bool hasRockTag = HasTerrainTag(terrain, "Rock");
            bool hasRockName = TerrainNameContains(terrain, "rock", "stone", "granite", "marble", "slate", "rubble");

            if (terrain.categoryType.Equals(TerrainDef.TerrainCategoryType.Sand)) {
                return defaultSpawnInformation.With(map, position);
            }
        }

        bool thingChecks = map!.thingGrid.ThingsListAt(position)
            .Select(thing => thing.def.defName)
            .Any(defName => defName.StartsWith("Sand") || defName.Contains("Sandstone"));

        return thingChecks ? defaultSpawnInformation.With(map, position) : null;
    }
}