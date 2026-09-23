using Cosmere.System.Roshar.Comp.Map;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The Weeping closes the last two weeks of the year to highstorms, and a storm that rolls
///     into it gets moved past it. These pin how far it moves.
/// </summary>
/// <remarks>
///     A storm was being pushed by the rest of the year measured from the day the roll happened,
///     not from the day the storm would have landed. On a colony far from the Weeping that turned
///     a five day wait into most of a year, which is what BetaHub #18 reported as a 47.5 day
///     forecast.
///     <para>
///         The other half of that bug was reading the storm's date off <c>TicksGame</c> while the
///         readout read it off the map's local date. Two different calendars, so they disagreed by
///         however far into a year the colony landed. That half needs <c>Find.TickManager</c> and
///         <c>Find.WorldGrid</c>, so it cannot be pinned here.
///     </para>
/// </remarks>
[TestClass]
public class WeepingScheduleTests {
    [TestMethod]
    public void TheWeepingCoversTheLastFourteenDays() {
        Assert.IsFalse(WeepingSchedule.IsWeepingDay(0), "day 0 is not the Weeping");
        Assert.IsFalse(WeepingSchedule.IsWeepingDay(45), "day 45 is the last clear day");
        Assert.IsTrue(WeepingSchedule.IsWeepingDay(46), "day 46 opens the Weeping");
        Assert.IsTrue(WeepingSchedule.IsWeepingDay(59), "day 59 is the last day of the year");
    }

    [TestMethod]
    public void AStormWaitsOnlyTheRestOfTheWeeping() {
        Assert.AreEqual(14, WeepingSchedule.DaysToClearWeeping(46), "a storm on the first day waits it all out");
        Assert.AreEqual(10, WeepingSchedule.DaysToClearWeeping(50), "a storm four days in waits ten");
        Assert.AreEqual(1, WeepingSchedule.DaysToClearWeeping(59), "a storm on the last day waits one");
    }

    [TestMethod]
    public void TheWaitNeverRunsPastOneWeeping() {
        for (int day = WeepingSchedule.StartDay; day < WeepingSchedule.DaysPerYear; day++) {
            int wait = WeepingSchedule.DaysToClearWeeping(day);

            Assert.IsTrue(wait > 0, $"a storm on day {day} has to move at least a day");
            Assert.IsTrue(
                wait <= WeepingSchedule.DaysPerYear - WeepingSchedule.StartDay,
                $"a storm on day {day} waits {wait} days, longer than the Weeping itself"
            );
        }
    }
}
