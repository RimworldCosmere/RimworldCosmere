using Cosmere.Resources;
using Cosmere.Resources.Def;
using Cosmere.Roshar.LesserSpren.ParticleSystem;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.LesserSpren.SprenControllers;

public class JoysprenController : DynamicSprenController {
    public override SprenType sprenType => SprenType.Joyspren;
    public override bool isEnabled => true;
    public override float cellSpawnChance => 0.3f;
    public override int minParticlesPerCell => 1;
    public override int maxParticlesPerCell => 3;
    public override List<SprenSpawnInformation> validSpawnInfo => activeSpawnInfo;

    public override List<GemDef> compatibleGemTypes => [
        GemDefOf.Diamond,
        GemDefOf.Heliodor,
        GemDefOf.Topaz,
        GemDefOf.Emerald,
    ];

    public override float captureRarityMultiplier => 0.7f;

    // Visual configuration
    public override Color sprenColor => new Color(1f, 0.9f, 0.3f, 0.8f);

    public override List<SprenSpawnInformation> GetDynamicSpawnInfo(Map? map) {
        if (map == null) return [];

        // Find pawns with high mood
        IEnumerable<Pawn> happyPawns = map.mapPawns.FreeColonistsSpawned
            .Where(pawn => pawn?.needs?.mood?.CurLevel > 0.75f);

        return happyPawns.Select(pawn => defaultSpawnInformation.With(map, pawn.Position)).ToList();
    }
}