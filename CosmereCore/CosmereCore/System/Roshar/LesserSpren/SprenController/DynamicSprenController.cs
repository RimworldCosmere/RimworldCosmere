using Verse;

namespace Cosmere.System.Roshar.LesserSpren.SprenController;

/// <summary>
///     Abstract base class for dynamic spren that change based on game state.
///     These spren respond to pawn emotions, events, or other dynamic conditions.
/// </summary>
public abstract class DynamicSprenController : BaseSprenController {
    // Dynamic spren have shorter refresh intervals to respond to changing conditions
    public override IntRange validInfoRefreshInterval => new IntRange(int.MaxValue, int.MaxValue);

    public override IntRange activeInfoRefreshInterval => new IntRange(0, 0);

    public override IReadOnlyCollection<SprenSpawnInformation> validSpawnInfo => activeSpawnInfo;

    public override SprenSpawnInformation? GetSprenSpawnInformation(IntVec3 position, Map? map) {
        return GetDynamicSpawnInfo(map).Find(i => i.position.Equals(position));
    }

    protected override void RefreshValidInfo(Map map) { }

    protected override void RefreshActiveInfo(Map map) {
        activeSpawnInfoInt.RemoveWhere(info => GenTicks.TicksGame > info.cleanupTick);

        foreach (SprenSpawnInformation? info in GetDynamicSpawnInfo(map).Where(i => Rand.Chance(i.spawnChance))) {
            activeSpawnInfoInt.Add(info.With(cleanupTick: GenTicks.TicksGame + lifetime.max.SecondsToTicks()));
        }
    }
}
