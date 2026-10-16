using System.Text;
using Cosmere.Core;
using Cosmere.Core.Def;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.LesserSpren.SprenController;

public class FearsprenController : DynamicSprenController {
    public override SprenType SprenType => SprenType.Fearspren;

    public override bool IsEnabled => true;

    public override float CellSpawnChance => 0.7f;

    public override int MinParticlesPerCell => 1;

    public override int MaxParticlesPerCell => 4;

    public override FloatRange Lifetime => new FloatRange(10f, 20f);

    public override List<GemDef> CompatibleGemTypes => [
        GemDefOf.Smokestone,
        GemDefOf.Amethyst,
        GemDefOf.Garnet,
        GemDefOf.Diamond,
    ];

    public override float CaptureRarityMultiplier => 0.6f;

    public override Color SprenColor => new Color(0.2f, 0.1f, 0.3f, 0.9f);

    public override float SprenSizeMultiplier => 0.8f;

    public override float EmissionRateMultiplier => 1.5f;

    public override List<SprenSpawnInformation> GetDynamicSpawnInfo(Map? map) {
        if (map == null) return [];

        List<SprenSpawnInformation> spawnInfos = [];

        foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned) {
            if (!pawn.Spawned || pawn.needs?.mood == null) continue;

            bool isFearful = false;
            float fearIntensity = CellSpawnChance; // Base spawn chance

            if (pawn.CurJobDef == RimWorld.JobDefOf.Flee ||
                pawn.CurJobDef == RimWorld.JobDefOf.FleeAndCower ||
                pawn.CurJobDef == RimWorld.JobDefOf.FleeAndCowerShort) {
                isFearful = true;
                fearIntensity = 0.9f; // Very high chance for fleeing pawns
            }

            float currentMood = pawn.needs.mood.CurLevel;
            float extremeBreakThreshold = pawn.mindState.mentalBreaker.BreakThresholdExtreme;
            float majorBreakThreshold = pawn.mindState.mentalBreaker.BreakThresholdMajor;

            if (currentMood <= extremeBreakThreshold + 0.05f) {
                // Very close to extreme mental break
                isFearful = true;
                fearIntensity = Mathf.Max(fearIntensity, 0.8f);
            } else if (currentMood <= majorBreakThreshold + 0.1f) {
                // Close to major mental break
                isFearful = true;
                fearIntensity = Mathf.Max(fearIntensity, 0.5f);
            } else if (currentMood < 0.25f) {
                // Generally very low mood (likely stressed/fearful)
                isFearful = true;
                fearIntensity = Mathf.Max(fearIntensity, 0.3f);
            }

            if (isFearful) {
                // Scale particle count based on fear intensity
                int minParticles = Mathf.RoundToInt(Mathf.Lerp(1f, MinParticlesPerCell, fearIntensity));
                int maxParticles = Mathf.RoundToInt(Mathf.Lerp(2f, MaxParticlesPerCell, fearIntensity));

                spawnInfos.Add(
                    DefaultSpawnInformation.With(
                        map,
                        pawn.Position,
                        fearIntensity,
                        minParticles,
                        maxParticles
                    )
                );
            }
        }

        return spawnInfos;
    }

    public override string DebugStringAt(IntVec3 position) {
        Map? map = Find.CurrentMap;
        if (map == null) return string.Empty;

        Pawn? pawn = map.thingGrid.ThingsAt(position).OfType<Pawn>().FirstOrDefault();
        if (pawn?.needs?.mood == null) return string.Empty;

        StringBuilder debug = new StringBuilder();
        debug.AppendLine($"  Current Mood: {pawn.needs.mood.CurLevel:F2}");
        debug.AppendLine($"  Extreme Break Threshold: {pawn.mindState.mentalBreaker.BreakThresholdExtreme:F2}");
        debug.AppendLine($"  Major Break Threshold: {pawn.mindState.mentalBreaker.BreakThresholdMajor:F2}");
        debug.AppendLine($"  Current Job: {pawn.CurJob?.def?.defName ?? "None"}");
        debug.AppendLine($"  Is Fleeing: {(pawn.CurJob?.def == RimWorld.JobDefOf.Flee ? "Yes" : "No")}");

        bool isFearful = false;
        float fearIntensity = CellSpawnChance;

        if (pawn.CurJob?.def == RimWorld.JobDefOf.Flee) {
            isFearful = true;
            fearIntensity = 0.9f;
        }

        float currentMood = pawn.needs.mood.CurLevel;
        float extremeThreshold = pawn.mindState.mentalBreaker.BreakThresholdExtreme;
        float majorThreshold = pawn.mindState.mentalBreaker.BreakThresholdMajor;

        if (currentMood <= extremeThreshold + 0.05f) {
            isFearful = true;
            fearIntensity = Mathf.Max(fearIntensity, 0.8f);
        } else if (currentMood <= majorThreshold + 0.1f) {
            isFearful = true;
            fearIntensity = Mathf.Max(fearIntensity, 0.5f);
        } else if (currentMood < 0.25f) {
            isFearful = true;
            fearIntensity = Mathf.Max(fearIntensity, 0.3f);
        }

        debug.AppendLine($"  Fear Detected: {(isFearful ? "Yes" : "No")}");
        if (isFearful) {
            debug.AppendLine($"  Fear Intensity: {fearIntensity:P1}");
        }

        return debug.ToString();
    }
}
