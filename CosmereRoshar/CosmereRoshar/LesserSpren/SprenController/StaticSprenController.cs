using Verse;

namespace Cosmere.Roshar.LesserSpren.SprenController;

/// <summary>
///     Abstract base class for static/nature spren that don't change frequently.
///     These spren are based on terrain and environmental features.
/// </summary>
public abstract class StaticSprenController : BaseSprenController {
    // Static spren have longer refresh intervals since terrain doesn't change often
    public override IntRange validInfoRefreshInterval => new IntRange(
        GenTicks.SecondsToTicks(600), // 10 minutes
        GenTicks.SecondsToTicks(3600) // 60 minutes
    );

    public override IntRange activeInfoRefreshInterval => new IntRange(
        GenTicks.SecondsToTicks(240), // 4 minutes  
        GenTicks.SecondsToTicks(480) // 8 minutes
    );
}