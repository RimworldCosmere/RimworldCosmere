using Cosmere.Core.Ability.Autocast;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Covers what an autocast rule does to an ability once its triggers stop passing.
/// </summary>
[TestClass]
public class AutocastDecisionTests {
    [TestMethod]
    public void SustainedAbilityIsReleasedWhenTriggersStop() {
        Assert.AreEqual(
            AutocastAction.TurnOff,
            AutocastDecision.For(
                toggleable: true,
                active: true,
                triggersPass: false,
                releaseOnStop: true,
                autocastLit: true,
                targetRequired: false
            )
        );
    }

    /// <summary>
    ///     A targeted toggle used to be skipped before it reached the release branch, so soothe and
    ///     riot stayed up for good once autocast started them.
    /// </summary>
    [TestMethod]
    public void NeedingATargetDoesNotBlockTheRelease() {
        Assert.AreEqual(
            AutocastAction.TurnOff,
            AutocastDecision.For(
                toggleable: true,
                active: true,
                triggersPass: false,
                releaseOnStop: true,
                autocastLit: true,
                targetRequired: true
            )
        );
    }

    [TestMethod]
    public void SustainedAbilityStaysUpWithoutReleaseOnStop() {
        Assert.AreEqual(
            AutocastAction.None,
            AutocastDecision.For(
                toggleable: true,
                active: true,
                triggersPass: false,
                releaseOnStop: false,
                autocastLit: true,
                targetRequired: false
            )
        );
    }

    [TestMethod]
    public void RunningAbilityIsNotCastAgainWhileTriggersPass() {
        Assert.AreEqual(
            AutocastAction.None,
            AutocastDecision.For(
                toggleable: true,
                active: true,
                triggersPass: true,
                releaseOnStop: true,
                autocastLit: true,
                targetRequired: false
            )
        );
    }

    [TestMethod]
    public void IdleAbilityCastsWhenTriggersPass() {
        Assert.AreEqual(
            AutocastAction.Cast,
            AutocastDecision.For(
                toggleable: true,
                active: false,
                triggersPass: true,
                releaseOnStop: true,
                autocastLit: false,
                targetRequired: false
            )
        );

        Assert.AreEqual(
            AutocastAction.Cast,
            AutocastDecision.For(
                toggleable: false,
                active: false,
                triggersPass: true,
                releaseOnStop: false,
                autocastLit: false,
                targetRequired: false
            )
        );
    }

    // Autocast has no target to pick, so it can only ever stop one of these.
    [TestMethod]
    public void TargetedAbilityIsNeverStarted() {
        Assert.AreEqual(
            AutocastAction.None,
            AutocastDecision.For(
                toggleable: true,
                active: false,
                triggersPass: true,
                releaseOnStop: true,
                autocastLit: false,
                targetRequired: true
            )
        );
    }

    [TestMethod]
    public void NothingHappensWhileTriggersFailAndTheAbilityIsIdle() {
        Assert.AreEqual(
            AutocastAction.None,
            AutocastDecision.For(
                toggleable: true,
                active: false,
                triggersPass: false,
                releaseOnStop: true,
                autocastLit: false,
                targetRequired: false
            )
        );
    }

    /// <summary>
    ///     The bug in 992c4ad4: a seeded rule with a Drafted trigger put out a pewter burn the
    ///     player had lit on an undrafted colonist. Autocast only releases what it started.
    /// </summary>
    [TestMethod]
    public void PlayerLitAbilityIsLeftAloneWhenTriggersStop() {
        Assert.AreEqual(
            AutocastAction.None,
            AutocastDecision.For(
                toggleable: true,
                active: true,
                triggersPass: false,
                releaseOnStop: true,
                autocastLit: false,
                targetRequired: false
            )
        );
    }

    [TestMethod]
    public void PlayerLitAbilityIsNotCastAgainWhileTriggersPass() {
        Assert.AreEqual(
            AutocastAction.None,
            AutocastDecision.For(
                toggleable: true,
                active: true,
                triggersPass: true,
                releaseOnStop: true,
                autocastLit: false,
                targetRequired: false
            )
        );
    }

    // nothing to turn off on a one-shot, so ownership cannot reach the release branch.
    [TestMethod]
    public void NonToggleableAbilityIsNeverTurnedOff() {
        Assert.AreEqual(
            AutocastAction.None,
            AutocastDecision.For(
                toggleable: false,
                active: true,
                triggersPass: false,
                releaseOnStop: true,
                autocastLit: true,
                targetRequired: false
            )
        );
    }

    [TestMethod]
    public void NonToggleableAbilityFiresAgainWhileTriggersPass() {
        Assert.AreEqual(
            AutocastAction.Cast,
            AutocastDecision.For(
                toggleable: false,
                active: true,
                triggersPass: true,
                releaseOnStop: true,
                autocastLit: true,
                targetRequired: false
            )
        );
    }
}
