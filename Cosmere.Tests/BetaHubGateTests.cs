using Cosmere.Core.BetaHub;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Covers the three conditions that together decide whether the feedback UI exists.
/// </summary>
[TestClass]
public class BetaHubGateTests {
    [TestMethod]
    public void PrereleaseRevisionsAreBeta() {
        Assert.IsTrue(BetaHubGate.IsBetaRevision("2.0.0-beta.23"));
        Assert.IsTrue(BetaHubGate.IsBetaRevision("10.1.4-beta.1"));
    }

    [TestMethod]
    public void StableAndAlphaRevisionsAreNotBeta() {
        Assert.IsFalse(BetaHubGate.IsBetaRevision("2.0.0"));
        Assert.IsFalse(BetaHubGate.IsBetaRevision("2.0.0-alpha.3"));
    }

    [TestMethod]
    public void AnUnsetRevisionIsNotBeta() {
        Assert.IsFalse(BetaHubGate.IsBetaRevision(null));
        Assert.IsFalse(BetaHubGate.IsBetaRevision(string.Empty));
    }

    /// <summary>
    ///     A contributor building from a clean checkout has no token, and must get a
    ///     working mod with no feedback UI rather than a broken button.
    /// </summary>
    [TestMethod]
    public void AMissingTokenHidesTheUiOnABetaBuild() {
        Assert.IsFalse(BetaHubGate.ShouldShow("2.0.0-beta.23", string.Empty, true));
        Assert.IsFalse(BetaHubGate.ShouldShow("2.0.0-beta.23", null, true));
    }

    [TestMethod]
    public void TheSettingsToggleHidesTheUi() {
        Assert.IsFalse(BetaHubGate.ShouldShow("2.0.0-beta.23", "tkn-abc", false));
    }

    [TestMethod]
    public void AStableBuildHidesTheUiEvenWithATokenAndTheToggleOn() {
        Assert.IsFalse(BetaHubGate.ShouldShow("2.0.0", "tkn-abc", true));
    }

    [TestMethod]
    public void AllThreeConditionsMetShowsTheUi() {
        Assert.IsTrue(BetaHubGate.ShouldShow("2.0.0-beta.23", "tkn-abc", true));
    }
}
