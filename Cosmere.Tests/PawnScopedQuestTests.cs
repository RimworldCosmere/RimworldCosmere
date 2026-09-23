using System.Collections.Generic;
using Cosmere.Core.Quest;
using Cosmere.Core.Quest.Prereq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards the pawn-scoped half of quest eligibility: dying in the Nightwatcher's Valley
///     must burn that quest for one colonist only.
/// </summary>
[TestClass]
public class PawnScopedQuestTests {
    private const int Kaladin = 1001;
    private const int Shallan = 1002;

    private static QuestWorldState CalledState() {
        return new QuestWorldState {
            currentTick = 100 * CosmereQuestEligibility.TicksPerDay,
            daysElapsed = 100,
            freeColonistCount = 8,
            enabledShards = new HashSet<string> { "Honor", "Cultivation" },
            subjectPawnId = Kaladin,
            subjectHediffs = new HashSet<string> { "Cosmere_Roshar_Hediff_BondsmithCalling_Stormfather" },
        };
    }

    [TestMethod]
    public void HediffPrereqReadsTheSubjectSnapshot() {
        HediffPrereq prereq = new HediffPrereq {
            hediffDefName = "Cosmere_Roshar_Hediff_BondsmithCalling_Stormfather",
        };
        Assert.IsTrue(prereq.IsMet(CalledState()));

        QuestWorldState uncalled = CalledState();
        uncalled.subjectHediffs.Clear();
        Assert.IsFalse(prereq.IsMet(uncalled));
    }

    [TestMethod]
    public void HediffPrereqIsNotMetWhenThereIsNoSubject() {
        QuestWorldState colonyScoped = CalledState();
        colonyScoped.subjectPawnId = 0;
        colonyScoped.subjectHediffs.Clear();

        HediffPrereq prereq = new HediffPrereq { hediffDefName = "anything" };
        Assert.IsFalse(prereq.IsMet(colonyScoped));
    }

    [TestMethod]
    public void RadiantOrderPrereqChecksOrderAndIdeal() {
        QuestWorldState state = CalledState();
        state.bondedOrders["Windrunner"] = 3;

        Assert.IsTrue(new RadiantOrderPrereq { order = "Windrunner", minIdeal = 2 }.IsMet(state));
        Assert.IsFalse(new RadiantOrderPrereq { order = "Windrunner", minIdeal = 4 }.IsMet(state));
        Assert.IsFalse(new RadiantOrderPrereq { order = "Skybreaker", minIdeal = 1 }.IsMet(state));
    }

    [TestMethod]
    public void RadiantOrderPrereqWithNoOrderAcceptsAnyRadiant() {
        QuestWorldState state = CalledState();
        Assert.IsFalse(new RadiantOrderPrereq { minIdeal = 1 }.IsMet(state));

        state.bondedOrders["Edgedancer"] = 1;
        Assert.IsTrue(new RadiantOrderPrereq { minIdeal = 1 }.IsMet(state));
    }

    [TestMethod]
    public void ABurnedQuestBurnsForOnePawnOnly() {
        QuestWorldState state = CalledState();
        state.pawnBurns["Cosmere_Roshar_Quest_BondNightwatcher"] = new HashSet<int> { Shallan };

        Assert.IsTrue(state.IsBurnedForPawn("Cosmere_Roshar_Quest_BondNightwatcher", Shallan));
        Assert.IsFalse(state.IsBurnedForPawn("Cosmere_Roshar_Quest_BondNightwatcher", Kaladin));
    }

    [TestMethod]
    public void AQuestWithNoPawnBurnsIsBurnedForNobody() {
        Assert.IsFalse(CalledState().IsBurnedForPawn("Cosmere_Roshar_Quest_BondNightwatcher", Kaladin));
    }
}
