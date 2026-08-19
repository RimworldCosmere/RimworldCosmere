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
                targetRequired: false
            )
        );
    }
}
