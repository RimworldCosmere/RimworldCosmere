using Cosmere.Core;
using Cosmere.Core.Def;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.LesserSpren.SprenController;

public class RainsprenController : DynamicSprenController {
    public override SprenType sprenType => SprenType.Rainspren;

    public override bool isEnabled => true;

    public override float cellSpawnChance => 0.0005f;

    public override int minParticlesPerCell => 2;

    public override int maxParticlesPerCell => 5;

    public override FloatRange lifetime => new FloatRange(10f, 20f);

    public override List<GemDef> compatibleGemTypes => [
        GemDefOf.Sapphire,
        GemDefOf.Amethyst,
        GemDefOf.Garnet,
        GemDefOf.Emerald,
    ];

    public override float captureRarityMultiplier => 1.3f;

    public override Color sprenColor => new Color(0.4f, 0.6f, 0.9f, 0.6f);

    public override List<SprenSpawnInformation> GetDynamicSpawnInfo(Map? map) {
        WeatherDef? currentWeather = map?.weatherManager.curWeather;
        if (currentWeather == null || currentWeather.rainRate <= 0f) {
            return [];
        }

        List<SprenSpawnInformation> cells = [];
        int cellsToCheck = map!.AllCells.Count() / 10;
        for (int i = 0; i < cellsToCheck; i++) {
            IntVec3 randomCell = CellFinder.RandomCell(map);
            if (!IsInBounds(randomCell, map)) continue;
            if (randomCell.Roofed(map)) continue;
            float rainIntensityMultiplier = Mathf.Clamp(currentWeather.rainRate * 2f, 0.3f, 1.5f);
            float spawnChance = cellSpawnChance * rainIntensityMultiplier;
            int minParticles = Mathf.RoundToInt(minParticlesPerCell * rainIntensityMultiplier);
            int maxParticles = Mathf.RoundToInt(maxParticlesPerCell * rainIntensityMultiplier);

            cells.Add(
                defaultSpawnInformation.With(
                    map,
                    randomCell,
                    spawnChance,
                    minParticles,
                    maxParticles
                )
            );
        }

        return cells;
    }

    public override SprenSpawnInformation? GetSprenSpawnInformation(IntVec3 position, Map? map) {
        if (!IsInBounds(position, map)) return null;

        WeatherDef currentWeather = map!.weatherManager.curWeather;
        if (currentWeather == null || currentWeather.rainRate <= 0f) return null;
        if (position.Roofed(map)) return null;

        float rainIntensityMultiplier = Mathf.Clamp(currentWeather.rainRate * 2f, 0.3f, 1.5f);
        (float spawnChance, int minParticles, int maxParticles) weatherCheck = (
            spawnChance: cellSpawnChance * rainIntensityMultiplier,
            minParticles: Mathf.RoundToInt(minParticlesPerCell * rainIntensityMultiplier),
            maxParticles: Mathf.RoundToInt(maxParticlesPerCell * rainIntensityMultiplier)
        );

        return defaultSpawnInformation.With(
            map,
            position,
            weatherCheck.spawnChance,
            weatherCheck.minParticles,
            weatherCheck.maxParticles
        );
    }
}
