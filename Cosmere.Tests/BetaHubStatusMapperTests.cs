using System;
using Cosmere.Core.BetaHub;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Covers the mapping from HTTP status to the outcome the dialog shows.
/// </summary>
[TestClass]
public class BetaHubStatusMapperTests {
    [TestMethod]
    public void TwoHundredsAreSuccess() {
        Assert.AreEqual(SubmitOutcome.Success, BetaHubStatusMapper.Map(200));
        Assert.AreEqual(SubmitOutcome.Success, BetaHubStatusMapper.Map(201));
        Assert.AreEqual(SubmitOutcome.Success, BetaHubStatusMapper.Map(204));
    }

    /// <summary>
    ///     Deleting a release answers 302. Any redirect here means the call worked.
    /// </summary>
    [TestMethod]
    public void RedirectsAreSuccess() {
        Assert.AreEqual(SubmitOutcome.Success, BetaHubStatusMapper.Map(302));
    }

    [TestMethod]
    public void ForbiddenIsTreatedAsTheDailyLimit() {
        Assert.AreEqual(SubmitOutcome.RateLimited, BetaHubStatusMapper.Map(403));
    }

    [TestMethod]
    public void UnprocessableIsARejection() {
        Assert.AreEqual(SubmitOutcome.Rejected, BetaHubStatusMapper.Map(422));
    }

    [TestMethod]
    public void ServerErrorsAndTransportFailuresAreUnreachable() {
        Assert.AreEqual(SubmitOutcome.Unreachable, BetaHubStatusMapper.Map(500));
        Assert.AreEqual(SubmitOutcome.Unreachable, BetaHubStatusMapper.Map(0));
    }

    [TestMethod]
    public void EveryOutcomeHasAMessageKey() {
        foreach (SubmitOutcome outcome in Enum.GetValues<SubmitOutcome>()) {
            Assert.IsFalse(string.IsNullOrEmpty(BetaHubStatusMapper.MessageKey(outcome)), outcome.ToString());
        }
    }
}
