using Cosmere;
﻿using Cosmere;
using Cosmere.Def;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.LesserSpren.SprenController;

public class DeathsprenController : DynamicSprenController {
    public override SprenType sprenType => SprenType.Deathspren;
    public override bool isEnabled => true;
    public override float cellSpawnChance => 1f;
    public override int minParticlesPerCell => 1;
    public override int maxParticlesPerCell => 4;

    public override List<GemDef> compatibleGemTypes => [
        GemDefOf.Smokestone,
        GemDefOf.Garnet,
        GemDefOf.Amethyst,
        GemDefOf.Diamond,
    ];

    public override float captureRarityMultiplier => 0.4f;

    public override Color sprenColor => new Color(0.1f, 0.1f, 0.1f, 0.9f);

    public override List<SprenSpawnInformation> GetDynamicSpawnInfo(Map? map) {
        return map?.listerThings.ThingsInGroup(ThingRequestGroup.Corpse)
                   .Where(corpse => corpse.GetRotStage() == RotStage.Fresh)
                   .Select(corpse => defaultSpawnInformation.With(map, corpse.Position)
                   )
                   .ToList() ??
               [];
    }
}