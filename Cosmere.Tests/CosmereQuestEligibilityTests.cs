using System.Collections.Generic;
using Cosmere.Core.Quest;
using Cosmere.Core.Quest.Prereq;
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
            era = "Cosmere_Scadrial_Era_PreCatacendre",
            freeColonistCount = 8,
            enabledShards = new HashSet<string> { "Preservation", "Ruin" },
        };
    }

    private static QuestCandidate Convoy() {
        return new QuestCandidate {
            defName = "Cosmere_Scadrial_Quest_AtiumCaravan",
            kind = QuestKind.Repeatable,
            eras = new List<string> { "Cosmere_Scadrial_Era_PreCatacendre" },
            cooldownDays = 25,
            selectionWeight = 1.2f,
        };
    }

    [TestMethod]
    public void BaselineRepeatableIsEligible() {
        Assert.IsTrue(CosmereQuestEligibility.IsEligible(Convoy(), BaseState()));
    }

    /// <summary>
    ///     The Pre-Catacendre scenario used to create no Final Empire, so Crystal in the Deep
    ///     was offered anyway and generated a site with no owner and no garrison.
    /// </summary>
    [TestMethod]
    public void MissingTargetFactionIsRejected() {
        QuestCandidate candidate = Convoy();
        candidate.targetFaction = "Cosmere_Scadrial_Faction_FinalEmpireNPC";

        Assert.IsFalse(CosmereQuestEligibility.IsEligible(candidate, BaseState()));
    }

    [TestMethod]
    public void PresentTargetFactionIsAccepted() {
        QuestCandidate candidate = Convoy();
        candidate.targetFaction = "Cosmere_Scadrial_Faction_FinalEmpireNPC";

        QuestWorldState state = BaseState();
        state.presentFactions.Add("Cosmere_Scadrial_Faction_FinalEmpireNPC");

        Assert.IsTrue(CosmereQuestEligibility.IsEligible(candidate, state));
    }

    [TestMethod]
    public void EraMismatchIsRejected() {
        QuestWorldState state = BaseState();
        state.era = "Cosmere_Scadrial_Era_PostCatacendre";
        Assert.IsFalse(CosmereQuestEligibility.IsEligible(Convoy(), state));
    }

    [TestMethod]
    public void EraAnyMatchesBothEras() {
        QuestCandidate anyEra = Convoy();
        anyEra.eras = null;

        QuestWorldState pre = BaseState();
        QuestWorldState post = BaseState();
        post.era = "Cosmere_Scadrial_Era_PostCatacendre";
        QuestWorldState noEra = BaseState();
        noEra.era = null;

        Assert.IsTrue(CosmereQuestEligibility.IsEligible(anyEra, pre));
        Assert.IsTrue(CosmereQuestEligibility.IsEligible(anyEra, post));
        Assert.IsTrue(CosmereQuestEligibility.IsEligible(anyEra, noEra));
    }

    [TestMethod]
    public void EraIsRejectedWhenWorldEraIsNull() {
        QuestWorldState state = BaseState();
        state.era = null;
        Assert.IsFalse(CosmereQuestEligibility.IsEligible(Convoy(), state));
    }

    [TestMethod]
    public void CandidateWithMultipleErasMatchesEitherButNotAThird() {
        QuestCandidate multiEra = Convoy();
        multiEra.eras = new List<string> {
            "Cosmere_Scadrial_Era_PreCatacendre",
            "Cosmere_Scadrial_Era_PostCatacendre",
        };

        QuestWorldState pre = BaseState();
        QuestWorldState post = BaseState();
        post.era = "Cosmere_Scadrial_Era_PostCatacendre";
        QuestWorldState alloyOfLaw = BaseState();
        alloyOfLaw.era = "Cosmere_Scadrial_Era_AlloyOfLaw";

        Assert.IsTrue(CosmereQuestEligibility.IsEligible(multiEra, pre));
        Assert.IsTrue(CosmereQuestEligibility.IsEligible(multiEra, post));
        Assert.IsFalse(CosmereQuestEligibility.IsEligible(multiEra, alloyOfLaw));
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
        QuestCandidate followUp = Convoy();
        followUp.kind = QuestKind.Capstone;
        followUp.defName = "Cosmere_Scadrial_Quest_FollowUpCapstone";
        followUp.requiredCapstone = "Cosmere_Scadrial_Quest_PitsOfHathsin";

        Assert.IsFalse(CosmereQuestEligibility.IsEligible(followUp, BaseState()));

        QuestWorldState done = BaseState();
        done.completedCapstones.Add("Cosmere_Scadrial_Quest_PitsOfHathsin");
        Assert.IsTrue(CosmereQuestEligibility.IsEligible(followUp, done));
    }

    [TestMethod]
    public void FilterKeepsOnlyEligibleCandidates() {
        QuestCandidate ok = Convoy();
        QuestCandidate wrongEra = Convoy();
        wrongEra.defName = "wrong";
        wrongEra.eras = new List<string> { "Cosmere_Scadrial_Era_PostCatacendre" };

        List<QuestCandidate> result =
            CosmereQuestEligibility.Filter(new List<QuestCandidate> { ok, wrongEra }, BaseState());

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(ok.defName, result[0].defName);
    }

    [TestMethod]
    public void RepeatableThatIsOtherwiseEligibleIsOfferable() {
        Assert.IsTrue(CosmereQuestEligibility.IsOfferableByStoryteller(Convoy(), BaseState()));
    }

    [TestMethod]
    public void EligibleCapstoneIsNeverOfferableByStorytellerEvenWhenNotFired() {
        QuestCandidate pits = Convoy();
        pits.kind = QuestKind.Capstone;
        pits.defName = "Cosmere_Scadrial_Quest_PitsOfHathsin";

        QuestWorldState state = BaseState();
        state.capstoneStates[pits.defName] = CapstoneState.NotFired;

        Assert.IsFalse(CosmereQuestEligibility.IsOfferableByStoryteller(pits, state));
    }

    [TestMethod]
    public void EligibleThreatIsNeverOfferableByStoryteller() {
        QuestCandidate raid = Convoy();
        raid.kind = QuestKind.Threat;
        raid.defName = "Cosmere_Scadrial_Quest_KolossRampage";

        Assert.IsFalse(CosmereQuestEligibility.IsOfferableByStoryteller(raid, BaseState()));
    }

    [TestMethod]
    public void FilterDropsACapstoneFromAMixedListButKeepsTheRepeatable() {
        QuestCandidate convoy = Convoy();
        QuestCandidate pits = Convoy();
        pits.kind = QuestKind.Capstone;
        pits.defName = "Cosmere_Scadrial_Quest_PitsOfHathsin";

        QuestWorldState state = BaseState();
        state.capstoneStates[pits.defName] = CapstoneState.NotFired;

        List<QuestCandidate> result =
            CosmereQuestEligibility.Filter(new List<QuestCandidate> { convoy, pits }, state);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(convoy.defName, result[0].defName);
    }

    [TestMethod]
    public void IsEligibleStillAcceptsAnEligibleCapstoneEvenThoughFilterWouldDropIt() {
        QuestCandidate pits = Convoy();
        pits.kind = QuestKind.Capstone;
        pits.defName = "Cosmere_Scadrial_Quest_PitsOfHathsin";

        QuestWorldState state = BaseState();
        state.capstoneStates[pits.defName] = CapstoneState.NotFired;

        Assert.IsTrue(CosmereQuestEligibility.IsEligible(pits, state));
    }

    [TestMethod]
    public void WeightIsClampedToNonNegative() {
        QuestCandidate negative = Convoy();
        negative.selectionWeight = -3f;
        Assert.AreEqual(0f, CosmereQuestEligibility.WeightOf(negative));
        Assert.AreEqual(1.2f, CosmereQuestEligibility.WeightOf(Convoy()), 0.0001f);
    }

    [TestMethod]
    public void FlagPrereqReadsTheWorldState() {
        FlagPrereq prereq = new FlagPrereq { flag = "HathsinLead" };

        QuestWorldState without = BaseState();
        Assert.IsFalse(prereq.IsMet(without));

        QuestWorldState with = BaseState();
        with.flags.Add("HathsinLead");
        Assert.IsTrue(prereq.IsMet(with));
    }

    [TestMethod]
    public void ShardPrereqRequiresEveryListedShard() {
        ShardPrereq prereq = new ShardPrereq {
            shards = new List<string> { "Preservation", "Ruin" },
        };
        Assert.IsTrue(prereq.IsMet(BaseState()));

        prereq.shards.Add("Harmony");
        Assert.IsFalse(prereq.IsMet(BaseState()));
    }

    [TestMethod]
    public void CapstoneStatePrereqDefaultsToRequiringCompletion() {
        CapstoneStatePrereq prereq = new CapstoneStatePrereq {
            questDefName = "Cosmere_Scadrial_Quest_PitsOfHathsin",
        };

        Assert.IsFalse(prereq.IsMet(BaseState()));

        QuestWorldState done = BaseState();
        done.capstoneStates["Cosmere_Scadrial_Quest_PitsOfHathsin"] = CapstoneState.Completed;
        Assert.IsTrue(prereq.IsMet(done));
    }

    [TestMethod]
    public void ColonistCountPrereqEnforcesItsMinimum() {
        ColonistCountPrereq prereq = new ColonistCountPrereq { minCount = 8 };

        QuestWorldState below = BaseState();
        below.freeColonistCount = 7;
        Assert.IsFalse(prereq.IsMet(below));

        Assert.IsTrue(prereq.IsMet(BaseState()));

        QuestWorldState above = BaseState();
        above.freeColonistCount = 9;
        Assert.IsTrue(prereq.IsMet(above));
    }

    [TestMethod]
    public void DaysElapsedPrereqEnforcesItsMinimum() {
        DaysElapsedPrereq prereq = new DaysElapsedPrereq { minDays = 100 };

        QuestWorldState before = BaseState();
        before.daysElapsed = 99;
        Assert.IsFalse(prereq.IsMet(before));

        Assert.IsTrue(prereq.IsMet(BaseState()));

        QuestWorldState after = BaseState();
        after.daysElapsed = 101;
        Assert.IsTrue(prereq.IsMet(after));
    }

    /// <summary>
    ///     The quest pool had no world dimension, so every Scadrial quest was offerable on a
    ///     Roshar save. Era filtering hid most of it - Roshar ships no EraDefs - but a quest
    ///     naming no era came through.
    /// </summary>
    [TestMethod]
    public void AQuestDoesNotOfferOnAnotherWorld() {
        QuestCandidate quest = Convoy();
        quest.world = "Scadrial";

        QuestWorldState scadrial = BaseState();
        scadrial.world = "Scadrial";
        Assert.IsTrue(CosmereQuestEligibility.IsEligible(quest, scadrial));

        QuestWorldState roshar = BaseState();
        roshar.world = "Roshar";
        Assert.IsFalse(
            CosmereQuestEligibility.IsEligible(quest, roshar),
            "A Scadrial quest must not be offered on a Roshar save."
        );
    }

    /// <summary>
    ///     Three unknowns all mean "do not narrow": a quest naming no world belongs to every
    ///     world, the cross-world sentinel reaches every shardworld, and a save that has not
    ///     chosen a world yet would otherwise get an empty pool.
    /// </summary>
    [TestMethod]
    public void TheWorldGateOnlyTurnsAwayAGenuineMismatch() {
        QuestWorldState roshar = BaseState();
        roshar.world = "Roshar";

        QuestCandidate worldless = Convoy();
        worldless.world = null;
        Assert.IsTrue(
            CosmereQuestEligibility.IsEligible(worldless, roshar),
            "A quest that names no world belongs to all of them."
        );

        QuestCandidate scadrialQuest = Convoy();
        scadrialQuest.world = "Scadrial";

        QuestWorldState sentinel = BaseState();
        sentinel.world = "UnnamedPlanet";
        sentinel.crossWorld = true;
        Assert.IsTrue(
            CosmereQuestEligibility.IsEligible(scadrialQuest, sentinel),
            "A cross-world save reaches every shardworld's quests."
        );

        QuestWorldState unchosen = BaseState();
        unchosen.world = null;
        Assert.IsTrue(
            CosmereQuestEligibility.IsEligible(scadrialQuest, unchosen),
            "Before a world is chosen, narrowing would silently empty the pool."
        );
    }

    /// <summary>
    ///     The Threat path has its own predicate and reaches IsEligible by a different route, so
    ///     the world gate has to hold there too.
    /// </summary>
    [TestMethod]
    public void ThreatsRespectTheWorldGate() {
        QuestCandidate threat = Convoy();
        threat.kind = QuestKind.Threat;
        threat.world = "Scadrial";

        QuestWorldState roshar = BaseState();
        roshar.world = "Roshar";

        Assert.IsFalse(CosmereQuestEligibility.IsDeliverableAsThreat(threat, roshar));

        QuestWorldState scadrial = BaseState();
        scadrial.world = "Scadrial";
        Assert.IsTrue(CosmereQuestEligibility.IsDeliverableAsThreat(threat, scadrial));
    }
}
