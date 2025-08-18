using Verse;

namespace Cosmere.Roshar.LesserSpren.SprenControllers;

/// <summary>
///     Abstract base class for static/nature spren that don't change frequently.
///     These spren are based on terrain and environmental features.
/// </summary>
public abstract class StaticSprenController : BaseSprenController {
    public override bool isNatureSpren => true;

    // Static spren have longer refresh intervals since terrain doesn't change often
    protected override IntRange validInfoRefreshInterval => new IntRange(
        GenTicks.SecondsToTicks(600), // 10 minutes
        GenTicks.SecondsToTicks(3600) // 60 minutes
    );

    protected override IntRange activeInfoRefreshInterval => new IntRange(
        GenTicks.SecondsToTicks(240), // 4 minutes  
        GenTicks.SecondsToTicks(480) // 8 minutes
    );

    // Default implementation for static spren - scan all cells
    protected override void RefreshValidInfo(Map map) {
        validSpawnInfoInt.Clear();

        foreach (IntVec3 cell in map.AllCells) {
            SprenSpawnInformation? spawnInfo = GetSprenSpawnInformation(cell, map);
            if (spawnInfo != null) {
                validSpawnInfoInt.Add(spawnInfo);
            }
        }
    }

    // Default implementation for static spren - random selection from valid cells
    protected override void RefreshActiveInfo(Map map) {
        activeSpawnInfoInt.Clear();

        foreach (SprenSpawnInformation? info in validSpawnInfoInt) {
            if (Rand.Chance(cellSpawnChance)) {
                activeSpawnInfoInt.Add(info);
            }
        }
    }
}