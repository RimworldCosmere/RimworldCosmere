using System.Collections.Generic;
using Cosmere.Core.Quest.Reward;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards RolledReward.ConfigError. Each failure mode must name the actual defect - a
///     blank key must not be reported as a sum mismatch just because the weights happen to
///     total 100 - and duplicate branch keys must be rejected outright, since Give fires the
///     first match and a later branch sharing a key would be permanently unreachable while
///     still consuming roll weight.
/// </summary>
[TestClass]
public class RolledRewardTests {
    private static QuestReward Reward() {
        return new FlagReward();
    }

    [TestMethod]
    public void NoBranchesIsInvalid() {
        RolledReward reward = new RolledReward { branches = new List<RolledRewardBranch>() };
        Assert.AreEqual("RolledReward has no branches.", reward.ConfigError());
    }

    [TestMethod]
    public void BranchWithNoRewardReportsMissingReward() {
        RolledReward reward = new RolledReward {
            branches = new List<RolledRewardBranch> {
                new RolledRewardBranch { key = "a", weight = 100, reward = null },
            },
        };

        Assert.AreEqual("RolledReward branch 'a' has no reward.", reward.ConfigError());
    }

    [TestMethod]
    public void BlankKeyWithWeightsSummingToOneHundredReportsKeyProblemNotSum() {
        RolledReward reward = new RolledReward {
            branches = new List<RolledRewardBranch> {
                new RolledRewardBranch { key = "a", weight = 50, reward = Reward() },
                new RolledRewardBranch { key = string.Empty, weight = 50, reward = Reward() },
            },
        };

        string? error = reward.ConfigError();
        Assert.AreEqual("RolledReward branch 1 has no key.", error);
    }

    [TestMethod]
    public void NullKeyReportsKeyProblem() {
        RolledReward reward = new RolledReward {
            branches = new List<RolledRewardBranch> {
                new RolledRewardBranch { key = null, weight = 100, reward = Reward() },
            },
        };

        Assert.AreEqual("RolledReward branch 0 has no key.", reward.ConfigError());
    }

    [TestMethod]
    public void ZeroWeightReportsWeightProblem() {
        RolledReward reward = new RolledReward {
            branches = new List<RolledRewardBranch> {
                new RolledRewardBranch { key = "a", weight = 0, reward = Reward() },
                new RolledRewardBranch { key = "b", weight = 100, reward = Reward() },
            },
        };

        Assert.AreEqual("RolledReward branch 'a' weight must be positive.", reward.ConfigError());
    }

    [TestMethod]
    public void NegativeWeightReportsWeightProblem() {
        RolledReward reward = new RolledReward {
            branches = new List<RolledRewardBranch> {
                new RolledRewardBranch { key = "a", weight = -10, reward = Reward() },
                new RolledRewardBranch { key = "b", weight = 110, reward = Reward() },
            },
        };

        Assert.AreEqual("RolledReward branch 'a' weight must be positive.", reward.ConfigError());
    }

    [TestMethod]
    public void DuplicateBranchKeysAreRejectedAndNameTheKey() {
        RolledReward reward = new RolledReward {
            branches = new List<RolledRewardBranch> {
                new RolledRewardBranch { key = "lerasium", weight = 50, reward = Reward() },
                new RolledRewardBranch { key = "cache", weight = 25, reward = Reward() },
                new RolledRewardBranch { key = "lerasium", weight = 25, reward = Reward() },
            },
        };

        string? error = reward.ConfigError();
        Assert.AreEqual("RolledReward has duplicate branch key 'lerasium'.", error);
    }

    [TestMethod]
    public void ValidTableReturnsNull() {
        RolledReward reward = new RolledReward {
            branches = new List<RolledRewardBranch> {
                new RolledRewardBranch { key = "a", weight = 40, reward = Reward() },
                new RolledRewardBranch { key = "b", weight = 60, reward = Reward() },
            },
        };

        Assert.IsNull(reward.ConfigError());
    }

    [TestMethod]
    public void WeightsNotSummingToOneHundredReportsSumWithActualTotal() {
        RolledReward reward = new RolledReward {
            branches = new List<RolledRewardBranch> {
                new RolledRewardBranch { key = "a", weight = 30, reward = Reward() },
                new RolledRewardBranch { key = "b", weight = 30, reward = Reward() },
            },
        };

        Assert.AreEqual("RolledReward weights must sum to exactly 100, got 60.", reward.ConfigError());
    }
}
