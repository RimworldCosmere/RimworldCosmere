using Cosmere.Resources;
using Cosmere.Resources.Def;
using Cosmere.Roshar.LesserSpren.ParticleSystem;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.LesserSpren.SprenController;

public class FlamesprenController : DynamicSprenController {
    public override SprenType sprenType => SprenType.Flamespren;
    public override bool isEnabled => true;
    public override float cellSpawnChance => 0.8f;
    public override int minParticlesPerCell => 3;
    public override int maxParticlesPerCell => 8;

    public override List<GemDef> compatibleGemTypes => [
        GemDefOf.Ruby,
        GemDefOf.Garnet,
        GemDefOf.Heliodor,
    ];

    public override float captureRarityMultiplier => 0.8f;

    public override Color sprenColor => new Color(1f, 0.4f, 0.1f, 0.9f);

    public override List<SprenSpawnInformation> GetDynamicSpawnInfo(Map? map) {
        List<SprenSpawnInformation> spawnInfos = [];

        foreach (Verse.Thing fire in map.listerThings.ThingsOfDef(RimWorld.ThingDefOf.Fire)) {
            spawnInfos.Add(defaultSpawnInformation.With(map, fire.Position));

            spawnInfos.AddRange(
                from adjacentCell in GenAdj.CellsAdjacent8Way(new TargetInfo(fire.Position, map))
                where IsInBounds(adjacentCell, map)
                select defaultSpawnInformation.With(
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

            // Lerp particle counts based on heat output
            // Typical values: campfire=21, heater=21, torch=7, brazier=15
            float lerpFactor =
                Mathf.Clamp01((heatPusher.Props.heatPerSecond - thing.AmbientTemperature - 5) / (30 - 5));

            // Scale particles from 2-4 (low heat) to 5-10 (high heat)
            int minParticles = Mathf.RoundToInt(Mathf.Lerp(1f, minParticlesPerCell, lerpFactor));
            int maxParticles = Mathf.RoundToInt(Mathf.Lerp(1f, maxParticlesPerCell, lerpFactor));

            spawnInfos.Add(
                defaultSpawnInformation.With(
                    map,
                    thing.Position,
                    cellSpawnChance,
                    minParticles,
                    maxParticles
                )
            );

            spawnInfos.AddRange(
                from adjacentCell in GenAdj.CellsAdjacent8Way(new TargetInfo(thing.Position, map))
                where IsInBounds(adjacentCell, map)
                select defaultSpawnInformation.With(
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