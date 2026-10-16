using Cosmere.Core;
using Cosmere.Core.Def;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.LesserSpren.SprenController;

public class SandsprenController : StaticSprenController {
    public override SprenType SprenType => SprenType.Sandspren;

    public override bool IsEnabled => true;

    public override float CellSpawnChance => 0.025f;

    public override int MinParticlesPerCell => 1;

    public override int MaxParticlesPerCell => 2;

    public override List<GemDef> CompatibleGemTypes => [
        GemDefOf.Garnet,
        GemDefOf.Diamond,
        GemDefOf.Topaz,
    ];

    public override float CaptureRarityMultiplier => 1.0f;

    public override Color SprenColor => new Color(0.9f, 0.8f, 0.5f, 0.6f);

    public override SprenSpawnInformation? GetSprenSpawnInformation(
        IntVec3 position,
        Map? map
    ) {
        if (!IsInBounds(position, map)) return null;

        TerrainDef? terrain = GetTerrain(position, map!);
        if (terrain != null && terrain.categoryType.Equals(TerrainDef.TerrainCategoryType.Sand)) {
            return DefaultSpawnInformation.With(map, position);
        }

        bool thingChecks = map!.thingGrid.ThingsListAt(position)
            .Select(thing => thing.def.defName)
            .Any(defName => defName.StartsWith("Sand") || defName.Contains("Sandstone"));

        return thingChecks ? DefaultSpawnInformation.With(map, position) : null;
    }
}
