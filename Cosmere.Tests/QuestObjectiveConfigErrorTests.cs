using Cosmere.Core.Quest.Objective;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards TimedWorkObjective.ConfigError. Only this objective's ConfigError is reachable
///     from the net9.0 test host: its body reads only requiredDaysOnSite and
///     reinforcementIntervalHours, both plain ints, so the JIT never needs to resolve a
///     RimWorld-typed field. CarryHomeObjective.ConfigError reads thingDef, a Verse.ThingDef,
///     which the JIT cannot resolve without Assembly-CSharp loaded - see task-12-report.md for
///     the probe that proved it throws FileNotFoundException.
///
///     ChoiceObjective.ConfigError was probed too: its body only reads a List&lt;QuestChoiceOption&gt;
///     count and a string key, neither RimWorld-typed. It still fails the same way, because
///     QuestChoiceOption implements Verse.IExposable - loading that type's interface map
///     requires resolving Verse.IExposable regardless of which method is called, throwing
///     FileNotFoundException for Assembly-CSharp before ConfigError's own body ever runs. See
///     task-13-report.md for the probe.
///
///     reinforcementIntervalHours = 0 must stay legal: Crystal in the Deep disables
///     reinforcements by setting it to 0, and QuestPart_TimedWork.QueueReinforcements already
///     treats a non-positive interval as "no reinforcements". Only a negative value is an error.
/// </summary>
[TestClass]
public class QuestObjectiveConfigErrorTests {
    [TestMethod]
    public void RequiredDaysOnSiteZeroIsInvalid() {
        TimedWorkObjective objective = new TimedWorkObjective { requiredDaysOnSite = 0 };
        Assert.AreEqual("TimedWorkObjective requiredDaysOnSite must be at least 1.", objective.ConfigError());
    }

    [TestMethod]
    public void RequiredDaysOnSiteNegativeIsInvalid() {
        TimedWorkObjective objective = new TimedWorkObjective { requiredDaysOnSite = -1 };
        Assert.AreEqual("TimedWorkObjective requiredDaysOnSite must be at least 1.", objective.ConfigError());
    }

    [TestMethod]
    public void RequiredDaysOnSiteOneIsValid() {
        TimedWorkObjective objective = new TimedWorkObjective { requiredDaysOnSite = 1, reinforcementIntervalHours = 4 };
        Assert.IsNull(objective.ConfigError());
    }

    [TestMethod]
    public void ReinforcementIntervalHoursNegativeIsInvalid() {
        TimedWorkObjective objective = new TimedWorkObjective {
            requiredDaysOnSite = 3,
            reinforcementIntervalHours = -1,
        };

        Assert.AreEqual("TimedWorkObjective reinforcementIntervalHours cannot be negative.", objective.ConfigError());
    }

    [TestMethod]
    public void ReinforcementIntervalHoursZeroIsValid() {
        TimedWorkObjective objective = new TimedWorkObjective {
            requiredDaysOnSite = 3,
            reinforcementIntervalHours = 0,
        };

        Assert.IsNull(objective.ConfigError());
    }

    [TestMethod]
    public void DefaultValuesAreValid() {
        TimedWorkObjective objective = new TimedWorkObjective();
        Assert.IsNull(objective.ConfigError());
    }
}
