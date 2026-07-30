using System.Collections.Generic;
using Cosmere.Core.Quest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards which quests the storyteller is allowed to offer. Every rule here is a case
///     where offering the wrong quest is visible to the player: a post-Catacendre quest in
///     the Final Empire, a burned capstone coming back, or a repeatable ignoring its
///     cooldown.
/// </summary>
[TestClass]
public class CosmereQuestEligibilityTests {
    private const int Day = CosmereQuestEligibility.TicksPerDay;

    private static QuestWorldState BaseState() {
        return new QuestWorldState {
            currentTick = 100 * Day,
            daysElapsed = 100,
            era = ScadrialEra.PreCatacendre,
            freeColonistCount = 8,
            enabledShards = new HashSet<string> { "Preservation", "Ruin" },
        };
    }

    private static QuestCandidate Convoy() {
        return new QuestCandidate {
            defName = "Cosmere_Scadrial_Quest_AtiumCaravan",
            kind = QuestKind.Repeatable,
            era = ScadrialEra.PreCatacendre,
            cooldownDays = 25,
            selectionWeight = 1.2f,
        };
    }

    [TestMethod]
    public void BaselineRepeatableIsEligible() {
        Assert.IsTrue(CosmereQuestEligibility.IsEligible(Convoy(), BaseState()));
    }

    [TestMethod]
    public void EraMismatchIsRejected() {
        QuestWorldState state = BaseState();
        state.era = ScadrialEra.PostCatacendre;
        Assert.IsFalse(CosmereQuestEligibility.IsEligible(Convoy(), state));
    }

    [TestMethod]
    public void EraAnyMatchesBothEras() {
        QuestCandidate anyEra = Convoy();
        anyEra.era = ScadrialEra.Any;

        QuestWorldState pre = BaseState();
        QuestWorldState post = BaseState();
        post.era = ScadrialEra.PostCatacendre;

        Assert.IsTrue(CosmereQuestEligibility.IsEligible(anyEra, pre));
        Assert.IsTrue(CosmereQuestEligibility.IsEligible(anyEra, post));
    }

    [TestMethod]
    public void MissingRequiredShardIsRejected() {
        QuestCandidate needsHarmony = Convoy();
        needsHarmony.requiredShards = new List<string> { "Harmony" };
        Assert.IsFalse(CosmereQuestEligibility.IsEligible(needsHarmony, BaseState()));
    }

    [TestMethod]
    public void MissingRequiredFlagIsRejected() {
        QuestCandidate needsLead = Convoy();
        needsLead.requiredFlags = new List<string> { "HathsinLead" };
        Assert.IsFalse(CosmereQuestEligibility.IsEligible(needsLead, BaseState()));

        QuestWorldState withFlag = BaseState();
        withFlag.flags.Add("HathsinLead");
        Assert.IsTrue(CosmereQuestEligibility.IsEligible(needsLead, withFlag));
    }

    [TestMethod]
    public void BurnedCapstoneIsNeverEligible() {
        QuestCandidate pits = Convoy();
        pits.kind = QuestKind.Capstone;
        pits.defName = "Cosmere_Scadrial_Quest_PitsOfHathsin";

        QuestWorldState state = BaseState();
        state.capstoneStates[pits.defName] = CapstoneState.Burned;

        Assert.IsFalse(CosmereQuestEligibility.IsEligible(pits, state));
    }

    [TestMethod]
    public void CompletedCapstoneIsNeverEligible() {
        QuestCandidate pits = Convoy();
        pits.kind = QuestKind.Capstone;
        pits.defName = "Cosmere_Scadrial_Quest_PitsOfHathsin";

        QuestWorldState state = BaseState();
        state.capstoneStates[pits.defName] = CapstoneState.Completed;

        Assert.IsFalse(CosmereQuestEligibility.IsEligible(pits, state));
    }

    [TestMethod]
    public void CapstoneAlreadyOfferedIsNotOfferedAgain() {
        QuestCandidate pits = Convoy();
        pits.kind = QuestKind.Capstone;
        pits.defName = "Cosmere_Scadrial_Quest_PitsOfHathsin";

        QuestWorldState state = BaseState();
        state.capstoneStates[pits.defName] = CapstoneState.Offered;

        Assert.IsFalse(CosmereQuestEligibility.IsEligible(pits, state));
    }

    [TestMethod]
    public void DeclinedCapstoneBecomesEligibleAgainOnlyAfterItsCooldown() {
        QuestCandidate pits = Convoy();
        pits.kind = QuestKind.Capstone;
        pits.defName = "Cosmere_Scadrial_Quest_PitsOfHathsin";
        pits.cooldownDays = 20;

        QuestWorldState state = BaseState();
        state.capstoneStates[pits.defName] = CapstoneState.NotFired;
        state.lastOfferedTick[pits.defName] = state.currentTick - 5 * Day;
        Assert.IsFalse(CosmereQuestEligibility.IsEligible(pits, state));

        state.lastOfferedTick[pits.defName] = state.currentTick - 21 * Day;
        Assert.IsTrue(CosmereQuestEligibility.IsEligible(pits, state));
    }

    [TestMethod]
    public void RepeatableRespectsItsCooldown() {
        QuestWorldState state = BaseState();
        state.lastOfferedTick[Convoy().defName] = state.currentTick - 10 * Day;
        Assert.IsFalse(CosmereQuestEligibility.IsEligible(Convoy(), state));

        state.lastOfferedTick[Convoy().defName] = state.currentTick - 26 * Day;
        Assert.IsTrue(CosmereQuestEligibility.IsEligible(Convoy(), state));
    }

    [TestMethod]
    public void ZeroCooldownIgnoresHowRecentlyItWasOffered() {
        QuestCandidate noCooldown = Convoy();
        noCooldown.cooldownDays = 0;

        QuestWorldState state = BaseState();
        state.lastOfferedTick[noCooldown.defName] = state.currentTick;

        Assert.IsTrue(CosmereQuestEligibility.IsEligible(noCooldown, state));
    }

    [TestMethod]
    public void MinColonistsAndMinDaysAreEnforced() {
        QuestCandidate pits = Convoy();
        pits.minColonists = 6;
        pits.minDaysElapsed = 40;

        QuestWorldState thin = BaseState();
        thin.freeColonistCount = 4;
        Assert.IsFalse(CosmereQuestEligibility.IsEligible(pits, thin));

        QuestWorldState early = BaseState();
        early.daysElapsed = 20;
        Assert.IsFalse(CosmereQuestEligibility.IsEligible(pits, early));

        Assert.IsTrue(CosmereQuestEligibility.IsEligible(pits, BaseState()));
    }

    [TestMethod]
    public void RequiredCapstoneMustBeCompleted() {
        QuestCandidate firstBead = Convoy();
        firstBead.kind = QuestKind.Capstone;
        firstBead.defName = "Cosmere_Scadrial_Quest_FirstLerasium";
        firstBead.requiredCapstone = "Cosmere_Scadrial_Quest_PitsOfHathsin";

        Assert.IsFalse(CosmereQuestEligibility.IsEligible(firstBead, BaseState()));

        QuestWorldState done = BaseState();
        done.completedCapstones.Add("Cosmere_Scadrial_Quest_PitsOfHathsin");
        Assert.IsTrue(CosmereQuestEligibility.IsEligible(firstBead, done));
    }

    [TestMethod]
    public void FilterKeepsOnlyEligibleCandidates() {
        QuestCandidate ok = Convoy();
        QuestCandidate wrongEra = Convoy();
        wrongEra.defName = "wrong";
        wrongEra.era = ScadrialEra.PostCatacendre;

        List<QuestCandidate> result =
            CosmereQuestEligibility.Filter(new List<QuestCandidate> { ok, wrongEra }, BaseState());

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(ok.defName, result[0].defName);
    }

    [TestMethod]
    public void WeightIsClampedToNonNegative() {
        QuestCandidate negative = Convoy();
        negative.selectionWeight = -3f;
        Assert.AreEqual(0f, CosmereQuestEligibility.WeightOf(negative));
        Assert.AreEqual(1.2f, CosmereQuestEligibility.WeightOf(Convoy()), 0.0001f);
    }
}
