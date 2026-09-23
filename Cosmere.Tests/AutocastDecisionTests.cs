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

    [TestMethod]
    public void ADormantRuleOwnsNothing() {
        Assert.IsFalse(AutocastDecision.NextHolding(
            dormant: true,
            abilityFound: true,
            toggleable: true,
            active: true,
            wasHolding: true,
            action: AutocastAction.None));
    }

    [TestMethod]
    public void LosingTheAbilityDropsTheClaim() {
        Assert.IsFalse(AutocastDecision.NextHolding(
            dormant: false,
            abilityFound: false,
            toggleable: true,
            active: true,
            wasHolding: true,
            action: AutocastAction.None));
    }

    [TestMethod]
    public void AnAbilityThatIsOffIsNobodys() {
        Assert.IsFalse(AutocastDecision.NextHolding(
            dormant: false,
            abilityFound: true,
            toggleable: true,
            active: false,
            wasHolding: true,
            action: AutocastAction.None));
    }

    [TestMethod]
    public void CastingAToggleClaimsIt() {
        Assert.IsTrue(AutocastDecision.NextHolding(
            dormant: false,
            abilityFound: true,
            toggleable: true,
            active: false,
            wasHolding: false,
            action: AutocastAction.Cast));
    }

    [TestMethod]
    public void CastingAOneShotClaimsNothing() {
        Assert.IsFalse(AutocastDecision.NextHolding(
            dormant: false,
            abilityFound: true,
            toggleable: false,
            active: false,
            wasHolding: false,
            action: AutocastAction.Cast));
    }

    [TestMethod]
    public void TurningItOffReleasesTheClaim() {
        Assert.IsFalse(AutocastDecision.NextHolding(
            dormant: false,
            abilityFound: true,
            toggleable: true,
            active: true,
            wasHolding: true,
            action: AutocastAction.TurnOff));
    }

    [TestMethod]
    public void AQuietPassKeepsTheClaimItAlreadyHad() {
        Assert.IsTrue(AutocastDecision.NextHolding(
            dormant: false,
            abilityFound: true,
            toggleable: true,
            active: true,
            wasHolding: true,
            action: AutocastAction.None));
    }

    [TestMethod]
    public void AQuietPassDoesNotInventAClaim() {
        Assert.IsFalse(AutocastDecision.NextHolding(
            dormant: false,
            abilityFound: true,
            toggleable: true,
            active: true,
            wasHolding: false,
            action: AutocastAction.None));
    }

    [TestMethod]
    public void ADormantRuleReleasesTheBurnItLit() {
        Assert.IsTrue(AutocastDecision.ReleasesWhenDormant(holding: true, releaseOnStop: true));
    }

    [TestMethod]
    public void ADormantRuleKeepsTheBurnWhenThePlayerTurnedReleaseOff() {
        Assert.IsFalse(AutocastDecision.ReleasesWhenDormant(holding: true, releaseOnStop: false));
    }

    [TestMethod]
    public void ADormantRuleNeverReleasesABurnItDidNotLight() {
        Assert.IsFalse(AutocastDecision.ReleasesWhenDormant(holding: false, releaseOnStop: true));
    }
}
