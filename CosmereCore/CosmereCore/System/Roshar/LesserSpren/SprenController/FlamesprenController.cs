using Cosmere.Core;
using Cosmere.Core.Def;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.LesserSpren.SprenController;

public class FlamesprenController : DynamicSprenController {
    public override SprenType SprenType => SprenType.Flamespren;

    public override bool IsEnabled => true;

    public override float CellSpawnChance => 0.8f;

    public override int MinParticlesPerCell => 3;

    public override int MaxParticlesPerCell => 8;

    public override List<GemDef> CompatibleGemTypes => [
        GemDefOf.Ruby,
        GemDefOf.Garnet,
        GemDefOf.Heliodor,
    ];

    public override float CaptureRarityMultiplier => 0.8f;

    public override Color SprenColor => new Color(1f, 0.4f, 0.1f, 0.9f);

    public override List<SprenSpawnInformation> GetDynamicSpawnInfo(Map? map) {
        List<SprenSpawnInformation> spawnInfos = [];

        foreach (Verse.Thing fire in map!.listerThings.ThingsOfDef(RimWorld.ThingDefOf.Fire)) {
            spawnInfos.Add(DefaultSpawnInformation.With(map, fire.Position));

            spawnInfos.AddRange(
                from adjacentCell in GenAdj.CellsAdjacent8Way(new TargetInfo(fire.Position, map))
                where IsInBounds(adjacentCell, map)
                select DefaultSpawnInformation.With(
                    map,
                    adjacentCell,
                    minParticles: 1,
                    maxParticles: 4
                )
            );
        }

        foreach (Verse.Thing thing in map.listerThings.AllThings) {
            if (!thing.TryGetComp(out CompHeatPusher heatPusher) || !heatPusher.ShouldPushHeatNow) continue;
            if (thing.TryGetComp(out CompRefuelable refuelable) && !refuelable.HasFuel) continue;
            if (thing.TryGetComp(out CompPowerTrader powerTrader) && !powerTrader.PowerOn) continue;

            // lerp particle count from heat output (campfire=21, heater=21, torch=7, brazier=15)
            float lerpFactor =
                Mathf.Clamp01((heatPusher.Props.heatPerSecond - thing.AmbientTemperature - 5) / (30 - 5));

            // Scale particles from 2-4 (low heat) to 5-10 (high heat)
            int minParticles = Mathf.RoundToInt(Mathf.Lerp(1f, MinParticlesPerCell, lerpFactor));
            int maxParticles = Mathf.RoundToInt(Mathf.Lerp(1f, MaxParticlesPerCell, lerpFactor));

            spawnInfos.Add(
                DefaultSpawnInformation.With(
                    map,
                    thing.Position,
                    CellSpawnChance,
                    minParticles,
                    maxParticles
                )
            );

            spawnInfos.AddRange(
                from adjacentCell in GenAdj.CellsAdjacent8Way(new TargetInfo(thing.Position, map))
                where IsInBounds(adjacentCell, map)
                select DefaultSpawnInformation.With(
                    position: adjacentCell,
                    spawnChance: 0.0005f,
                    minParticles: 1,
                    maxParticles: 1
                )
            );
        }

        return spawnInfos;
    }
}
