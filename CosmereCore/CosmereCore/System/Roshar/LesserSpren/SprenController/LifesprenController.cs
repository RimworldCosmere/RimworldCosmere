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
    public override SprenType SprenType => SprenType.Lifespren;

    public override bool IsEnabled => true;

    public override float CellSpawnChance => 0.7f;

    public override int MinParticlesPerCell => 2;

    public override int MaxParticlesPerCell => 5;

    public override List<GemDef> CompatibleGemTypes => [
        GemDefOf.Emerald,
        GemDefOf.Diamond,
        GemDefOf.Heliodor,
        GemDefOf.Topaz,
    ];

    public override float CaptureRarityMultiplier => 0.6f;

    public override Color SprenColor => new Color(0.1f, 0.9f, 0.1f, 0.8f);

    public override float SprenSizeMultiplier => 0.9f;

    public override List<SprenSpawnInformation> GetDynamicSpawnInfo(Map? map) {
        List<SprenSpawnInformation> spawnInfos = [];

        IReadOnlyList<Pawn> allPawns = map?.mapPawns.AllPawnsSpawned ?? [];
        spawnInfos.AddRange(
            from pawn in allPawns
            where pawn.ageTracker.AgeBiologicalTicks < GenDate.TicksPerDay
            select DefaultSpawnInformation.With(
                position: pawn.Position,
                spawnChance: 0.9f,
                minParticles: 5,
                maxParticles: 10
            )
        );

        List<Verse.Thing>? allPlants = map!.listerThings.ThingsInGroup(ThingRequestGroup.Plant);
        foreach (Verse.Thing? plant in allPlants) {
            if (plant is Plant { Growth: > 0.8f, LifeStage: PlantLifeStage.Growing }) {
                spawnInfos.Add(
                    DefaultSpawnInformation.With(
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
            select DefaultSpawnInformation.With(
                position: bed.Position,
                spawnChance: 0.7f,
                minParticles: 3,
                maxParticles: 6
            )
        );

        return spawnInfos;
    }
}
