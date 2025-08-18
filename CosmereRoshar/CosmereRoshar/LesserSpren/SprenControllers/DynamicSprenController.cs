using Verse;

namespace Cosmere.Roshar.LesserSpren.SprenControllers;

/// <summary>
///     Abstract base class for dynamic spren that change based on game state.
///     These spren respond to pawn emotions, events, or other dynamic conditions.
/// </summary>
public abstract class DynamicSprenController : BaseSprenController {
    public override bool isNatureSpren => false;

    // Dynamic spren have shorter refresh intervals to respond to changing conditions
    public override IntRange validInfoRefreshInterval => new IntRange(
        GenTicks.SecondsToTicks(60), // 1 minute
        GenTicks.SecondsToTicks(120) // 2 minutes
    );

    public override IntRange activeInfoRefreshInterval => new IntRange(
        GenTicks.SecondsToTicks(10), // 10 seconds
        GenTicks.SecondsToTicks(20) // 20 seconds  
    );

    // Dynamic spren typically implement their own custom refresh logic
    // based on pawns, events, or other game state
}