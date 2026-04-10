using Cosmere.Core;
using Cosmere.Core.Def;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using RimWorld;
using UnityEngine;
using Verse;
using Map = Verse.Map;
using Pawn = Verse.Pawn;
using ThingRequestGroup = Verse.ThingRequestGroup;

namespace Cosmere.System.Roshar.LesserSpren.SprenController;

public class LifesprenController : DynamicSprenController {
    public override SprenType sprenType => SprenType.Lifespren;
    public override bool isEnabled => true;
    public override float cellSpawnChance => 0.7f;
    public override int minParticlesPerCell => 2;
    public override int maxParticlesPerCell => 5;

    public override List<GemDef> compatibleGemTypes => [
        GemDefOf.Emerald,
        GemDefOf.Diamond,
        GemDefOf.Heliodor,
        GemDefOf.Topaz,
    ];

    public override float captureRarityMultiplier => 0.6f;

    public override Color sprenColor => new Color(0.1f, 0.9f, 0.1f, 0.8f);
    public override float sprenSizeMultiplier => 0.9f;

    public override List<SprenSpawnInformation> GetDynamicSpawnInfo(Map? map) {
        List<SprenSpawnInformation> spawnInfos = [];

        // Find recently born pawns
        IReadOnlyList<Pawn> allPawns = map?.mapPawns.AllPawnsSpawned ?? [];
        spawnInfos.AddRange(
            from pawn in allPawns
            where pawn.ageTracker.AgeBiologicalTicks < GenDate.TicksPerDay
            select defaultSpawnInformation.With(
                position: pawn.Position,
                spawnChance: 0.9f,
                minParticles: 5,
                maxParticles: 10
            )
        );

        // Find growing plants
        List<Verse.Thing>? allPlants = map!.listerThings.ThingsInGroup(ThingRequestGroup.Plant);
        foreach (Verse.Thing? plant in allPlants) {
            if (plant is Plant { Growth: > 0.8f, LifeStage: PlantLifeStage.Growing }) {
                spawnInfos.Add(
                    defaultSpawnInformation.With(
                        position: plant.Position,
                        spawnChance: 0.6f,
                        minParticles: 2,
                        maxParticles: 5
                    )
                );
            }
        }

        // Medical areas where healing occurs
        List<Verse.Thing>? allBeds = map.listerThings.ThingsOfDef(RimWorld.ThingDefOf.Bed);
        spawnInfos.AddRange(
            from bed in allBeds
            where bed.TryGetComp<CompAssignableToPawn>()
                      ?.AssignedPawnsForReading?.Any(p => p.health.HasHediffsNeedingTend()) ==
                  true
            select defaultSpawnInformation.With(
                position: bed.Position,
                spawnChance: 0.7f,
                minParticles: 3,
                maxParticles: 6
            )
        );

        return spawnInfos;
    }
}