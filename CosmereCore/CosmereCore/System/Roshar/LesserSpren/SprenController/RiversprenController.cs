using Cosmere;
using Cosmere.Def;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.LesserSpren.SprenController;

public class RiversprenController : StaticSprenController {
    public override SprenType sprenType => SprenType.Riverspren;
    public override bool isEnabled => true;
    public override float cellSpawnChance => 0.06f; // 6% chance per valid cell
    public override int minParticlesPerCell => 2;
    public override int maxParticlesPerCell => 4;
    protected override float maxSpreadDistance => 0.3f;
    protected override float movementSpeed => 3f;
    protected override float randomDirectionAmount => 0.15f;

    public override List<GemDef> compatibleGemTypes => [
        GemDefOf.Sapphire,
        GemDefOf.Zircon,
        GemDefOf.Diamond,
    ];

    public override float captureRarityMultiplier => 1.0f;
    public override Color sprenColor => new Color(0.3f, 0.7f, 0.9f, 0.9f); // River blue color

    public override SprenSpawnInformation? GetSprenSpawnInformation(
        IntVec3 position,
        Map? map
    ) {
        if (!IsInBounds(position, map)) return null;

        // Check terrain for river tag
        TerrainDef? terrain = GetTerrain(position, map);
        if (terrain == null) return null;

        return HasTerrainTag(terrain, "River") ? defaultSpawnInformation.With(map, position) : null;
    }
}