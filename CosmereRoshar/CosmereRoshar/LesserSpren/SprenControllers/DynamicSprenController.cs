using Verse;

namespace Cosmere.Roshar.LesserSpren.SprenControllers;

/// <summary>
///     Abstract base class for dynamic spren that change based on game state.
///     These spren respond to pawn emotions, events, or other dynamic conditions.
/// </summary>
public abstract class DynamicSprenController : BaseSprenController {
    // Dynamic spren have shorter refresh intervals to respond to changing conditions
    public override IntRange validInfoRefreshInterval => new IntRange(int.MaxValue, int.MaxValue);

    public override IntRange activeInfoRefreshInterval => new IntRange(0, 0);

    public override SprenSpawnInformation? GetSprenSpawnInformation(IntVec3 position, Map? map) {
        return GetDynamicSpawnInfo(map).Find(i => i.position.Equals(position));
    }

    protected override void RefreshValidInfo(Map map) { }

    protected override void RefreshActiveInfo(Map map) {
        activeSpawnInfoInt.Clear();

        activeSpawnInfo.AddRange(GetDynamicSpawnInfo(map));
    }
}