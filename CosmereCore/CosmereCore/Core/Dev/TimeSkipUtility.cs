using LudeonTK;
using RimWorld;
using Verse;

namespace Cosmere.Core.Dev;

/// <summary>
///     Moves the game clock without simulating what it skips. Day-gated content - a quest's
///     minDaysElapsed, a cooldown, a capstone's day trigger - is otherwise only reachable by
///     sitting on Ultrafast for real minutes per in-game day.
/// </summary>
[StaticConstructorOnStartup]
public static class TimeSkipUtility {
    private const int MaxSkipDays = 120;

    /// <summary>
    ///     Dialog_Slider is a fixed 130f tall and pins its buttons to inRect.yMax - 30, but the
    ///     slider sits below the label at CalcHeight(text); two label lines push it onto the buttons.
    /// </summary>
    private const float SliderExtraHeight = 40f;

    [DebugAction(
        "Cosmere/Core",
        "Skip forward N days...",
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void SkipDays() {
        Find.WindowStack.Add(
            new Dialog_Slider(
                days => $"Skip forward {days} day{(days == 1 ? string.Empty : "s")}\nCurrently day {GenDate.DaysPassed}",
                1,
                MaxSkipDays,
                Skip
            ) {
                extraBottomSpace = SliderExtraHeight,
            }
        );
    }

    [DebugAction(
        "Cosmere/Core",
        "Skip forward 1 day",
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void SkipOneDay() {
        Skip(1);
    }

    [DebugAction(
        "Cosmere/Core",
        "Skip forward 5 days",
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void SkipFiveDays() {
        Skip(5);
    }

    /// <summary>
    ///     Jumps ticksGameInt rather than running the ticks. 5 days is 300,000 ticks and running
    ///     them would lock the game for minutes, so nothing in between is simulated: no growth,
    ///     no needs, no incidents. Anything already scheduled for a tick inside the skipped
    ///     window fires in one burst on the next tick.
    /// </summary>
    private static void Skip(int days) {
        if (days <= 0) return;

        TickManager ticks = Find.TickManager;
        int before = GenDate.DaysPassed;
        ticks.DebugSetTicksGame(ticks.TicksGame + days * GenDate.TicksPerDay);

        Logger.Important($"Skipped {days} day(s): day {before} -> day {GenDate.DaysPassed}.");
        Messages.Message(
            $"Skipped to day {GenDate.DaysPassed}. Nothing in between was simulated.",
            MessageTypeDefOf.TaskCompletion,
            false
        );
    }
}
