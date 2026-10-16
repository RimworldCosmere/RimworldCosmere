using Cosmere.Core;
using Cosmere.Core.Def;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.LesserSpren.SprenController;

public class JoysprenController : DynamicSprenController {
    public override SprenType SprenType => SprenType.Joyspren;

    public override bool IsEnabled => true;

    public override float CellSpawnChance => 0.3f;

    public override int MinParticlesPerCell => 1;

    public override int MaxParticlesPerCell => 2;

    public override FloatRange Lifetime => new FloatRange(1f, 4f);

    public override List<GemDef> CompatibleGemTypes => [
        GemDefOf.Diamond,
        GemDefOf.Heliodor,
        GemDefOf.Topaz,
        GemDefOf.Emerald,
    ];

    public override float CaptureRarityMultiplier => 0.7f;

    public override Color SprenColor => new Color(1f, 0.9f, 0.3f, 0.8f);

    public override List<SprenSpawnInformation> GetDynamicSpawnInfo(Map? map) {
        if (map == null) return [];

        IEnumerable<Pawn> happyPawns = map.mapPawns.FreeColonistsSpawned
            .Where(pawn => pawn?.needs?.mood?.CurLevel > 0.75f);

        return [.. happyPawns.Select(pawn => DefaultSpawnInformation.With(map, pawn.Position))];
    }
}
