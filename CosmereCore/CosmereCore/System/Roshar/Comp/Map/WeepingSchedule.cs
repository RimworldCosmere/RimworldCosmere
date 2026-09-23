namespace Cosmere.System.Roshar.Comp.Map;

/// <summary>
///     The date arithmetic behind the Weeping, with no map or tick manager attached.
/// </summary>
/// <remarks>
///     Split out so it can be pinned by tests. <see cref="HighstormScheduler" /> is a
///     <c>MapComponent</c>, which the test project cannot load.
/// </remarks>
public static class WeepingSchedule {
    /// <summary>The day of year the Weeping settles in and highstorms stop.</summary>
    public const int StartDay = 46;

    /// <summary>Days in a RimWorld year.</summary>
    public const int DaysPerYear = 60;

    /// <summary>Whether a day of year falls inside the Weeping.</summary>
    /// <param name="dayOfYear">The day to test, 0 to <see cref="DaysPerYear" /> - 1.</param>
    /// <returns>True when the Weeping covers that day.</returns>
    public static bool IsWeepingDay(int dayOfYear) {
        return dayOfYear >= StartDay;
    }

    /// <summary>
    ///     How many whole days a storm has to wait to clear the Weeping it landed in.
    /// </summary>
    /// <remarks>
    ///     Measured from the storm's own day, never from today. Measuring from today pushed a
    ///     storm by the whole rest of the year rather than by the rest of the Weeping.
    /// </remarks>
    /// <param name="stormDayOfYear">The day of year the storm was going to land on.</param>
    /// <returns>Days from that storm's day to the turn of the year.</returns>
    public static int DaysToClearWeeping(int stormDayOfYear) {
        return DaysPerYear - stormDayOfYear;
    }
}
