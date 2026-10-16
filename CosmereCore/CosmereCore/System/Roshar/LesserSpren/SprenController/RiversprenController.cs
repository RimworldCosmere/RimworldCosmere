using Cosmere.Core;
using Cosmere.Core.Def;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.LesserSpren.SprenController;

public class RiversprenController : StaticSprenController {
    public override SprenType SprenType => SprenType.Riverspren;

    public override bool IsEnabled => true;

    public override float CellSpawnChance => 0.06f; // 6% chance per valid cell

    public override int MinParticlesPerCell => 2;

    public override int MaxParticlesPerCell => 4;

    protected override float MaxSpreadDistance => 0.3f;

    protected override float MovementSpeed => 3f;

    protected override float RandomDirectionAmount => 0.15f;

    public override List<GemDef> CompatibleGemTypes => [
        GemDefOf.Sapphire,
        GemDefOf.Zircon,
        GemDefOf.Diamond,
    ];

    public override float CaptureRarityMultiplier => 1.0f;

    public override Color SprenColor => new Color(0.3f, 0.7f, 0.9f, 0.9f); // River blue color

    public override SprenSpawnInformation? GetSprenSpawnInformation(
        IntVec3 position,
        Map? map
    ) {
        if (!IsInBounds(position, map)) return null;

        TerrainDef? terrain = GetTerrain(position, map!);
        if (terrain == null) return null;

        return HasTerrainTag(terrain, "River") ? DefaultSpawnInformation.With(map, position) : null;
    }
}
