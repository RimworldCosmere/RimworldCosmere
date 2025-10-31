using System.Text;
using Cosmere.Core;
using Cosmere.Core.Def;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using UnityEngine;
using Verse;
using JobDefOf = RimWorld.JobDefOf;

namespace Cosmere.System.Roshar.LesserSpren.SprenController;

public class FearsprenController : DynamicSprenController {
    public override SprenType sprenType => SprenType.Fearspren;
    public override bool isEnabled => true;
    public override float cellSpawnChance => 0.7f;
    public override int minParticlesPerCell => 1;
    public override int maxParticlesPerCell => 4;
    public override FloatRange lifetime => new FloatRange(10f, 20f);

    public override List<GemDef> compatibleGemTypes => [
        GemDefOf.Smokestone,
        GemDefOf.Amethyst,
        GemDefOf.Garnet,
        GemDefOf.Diamond,
    ];

    public override float captureRarityMultiplier => 0.6f;

    public override Color sprenColor => new Color(0.2f, 0.1f, 0.3f, 0.9f);
    public override float sprenSizeMultiplier => 0.8f;
    public override float emissionRateMultiplier => 1.5f;

    public override List<SprenSpawnInformation> GetDynamicSpawnInfo(Map? map) {
        if (map == null) return [];

        List<SprenSpawnInformation> spawnInfos = [];

        foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned) {
            if (pawn?.Position == null || pawn.needs?.mood == null) continue;

            bool isFearful = false;
            float fearIntensity = cellSpawnChance; // Base spawn chance

            // Check if pawn is actively fleeing
            if (pawn.CurJobDef == JobDefOf.Flee ||
                pawn.CurJobDef == JobDefOf.FleeAndCower ||
                pawn.CurJobDef == JobDefOf.FleeAndCowerShort) {
                isFearful = true;
                fearIntensity = 0.9f; // Very high chance for fleeing pawns
            }

            // Check mental break risk (stress/fear indicator)
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
                int minParticles = Mathf.RoundToInt(Mathf.Lerp(1f, minParticlesPerCell, fearIntensity));
                int maxParticles = Mathf.RoundToInt(Mathf.Lerp(2f, maxParticlesPerCell, fearIntensity));

                spawnInfos.Add(
                    defaultSpawnInformation.With(
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
        if (map == null) return "";

        Pawn? pawn = map.thingGrid.ThingsAt(position).OfType<Pawn>().FirstOrDefault();
        if (pawn?.needs?.mood == null) return "";

        StringBuilder debug = new StringBuilder();
        debug.AppendLine($"  Current Mood: {pawn.needs.mood.CurLevel:F2}");
        debug.AppendLine($"  Extreme Break Threshold: {pawn.mindState.mentalBreaker.BreakThresholdExtreme:F2}");
        debug.AppendLine($"  Major Break Threshold: {pawn.mindState.mentalBreaker.BreakThresholdMajor:F2}");
        debug.AppendLine($"  Current Job: {pawn.CurJob?.def?.defName ?? "None"}");
        debug.AppendLine($"  Is Fleeing: {(pawn.CurJob?.def == JobDefOf.Flee ? "Yes" : "No")}");

        // Calculate fear intensity for this pawn
        bool isFearful = false;
        float fearIntensity = cellSpawnChance;

        if (pawn.CurJob?.def == JobDefOf.Flee) {
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