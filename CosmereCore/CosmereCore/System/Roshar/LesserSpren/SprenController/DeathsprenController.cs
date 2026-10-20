using Cosmere.Core;
using Cosmere.Core.Def;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.LesserSpren.SprenController;

public class DeathsprenController : DynamicSprenController {
    public override SprenType SprenType => SprenType.Deathspren;

    public override bool IsEnabled => true;

    public override float CellSpawnChance => 1f;

    public override int MinParticlesPerCell => 1;

    public override int MaxParticlesPerCell => 4;

    public override List<GemDef> CompatibleGemTypes => [
        GemDefOf.Smokestone,
        GemDefOf.Garnet,
        GemDefOf.Amethyst,
        GemDefOf.Diamond,
    ];

    public override float CaptureRarityMultiplier => 0.4f;

    public override Color SprenColor => new Color(0.1f, 0.1f, 0.1f, 0.9f);

    public override List<SprenSpawnInformation> GetDynamicSpawnInfo(Map? map) {
        return map?.listerThings.ThingsInGroup(ThingRequestGroup.Corpse)
                   .Where(corpse => corpse.GetRotStage() == RotStage.Fresh)
                   .Select(corpse => DefaultSpawnInformation.With(map, corpse.Position)
                   )
                   .ToList() ??
               [];
    }
}
