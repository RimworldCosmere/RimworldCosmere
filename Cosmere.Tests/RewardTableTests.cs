using System.Collections.Generic;
using Cosmere.Core.Quest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards the rolled reward tables. Weights must sum to exactly 100 so a def author
///     cannot silently create a table that never picks its last entry, and a given seed
///     must always produce the same result so a reload cannot re-roll a resolved reward.
/// </summary>
[TestClass]
public class RewardTableTests {
    private static List<RewardTableEntry> HarmonyTable() {
        return new List<RewardTableEntry> {
            new RewardTableEntry { weight = 15, key = "lerasium" },
            new RewardTableEntry { weight = 15, key = "leratium" },
            new RewardTableEntry { weight = 15, key = "harmonium" },
            new RewardTableEntry { weight = 30, key = "payment" },
            new RewardTableEntry { weight = 25, key = "cache" },
        };
    }

    [TestMethod]
    public void HarmonyTableSumsToOneHundred() {
        Assert.AreEqual(100, RewardTable.TotalWeight(HarmonyTable()));
        Assert.IsTrue(RewardTable.IsValid(HarmonyTable()));
    }

    [TestMethod]
    public void TableThatDoesNotSumToOneHundredIsInvalid() {
        List<RewardTableEntry> bad = new List<RewardTableEntry> {
            new RewardTableEntry { weight = 50, key = "a" },
            new RewardTableEntry { weight = 40, key = "b" },
        };
        Assert.IsFalse(RewardTable.IsValid(bad));
    }

    [TestMethod]
    public void RollIsDeterministicForAGivenSeed() {
        string first = RewardTable.Roll(HarmonyTable(), 12345)!;
        string second = RewardTable.Roll(HarmonyTable(), 12345)!;
        Assert.AreEqual(first, second);
    }

    [TestMethod]
    public void EveryEntryIsReachable() {
        HashSet<string> seen = new HashSet<string>();
        for (int seed = 0; seed < 5000; seed++) {
            seen.Add(RewardTable.Roll(HarmonyTable(), seed)!);
        }

        Assert.AreEqual(5, seen.Count, "Every entry in a valid table must be reachable.");
    }

    [TestMethod]
    public void DistributionTracksWeightsWithinTolerance() {
        Dictionary<string, int> counts = new Dictionary<string, int>();
        const int rolls = 100000;
        for (int seed = 0; seed < rolls; seed++) {
            string key = RewardTable.Roll(HarmonyTable(), seed)!;
            counts.TryGetValue(key, out int n);
            counts[key] = n + 1;
        }

        AssertShare(counts, "payment", 0.30f, rolls);
        AssertShare(counts, "cache", 0.25f, rolls);
        AssertShare(counts, "lerasium", 0.15f, rolls);
    }

    private static void AssertShare(Dictionary<string, int> counts, string key, float expected, int rolls) {
        float actual = counts[key] / (float)rolls;
        Assert.IsTrue(
            actual > expected - 0.02f && actual < expected + 0.02f,
            $"{key} share was {actual:P2}, expected about {expected:P0}."
        );
    }

    [TestMethod]
    public void EmptyTableIsInvalidAndRollsToNull() {
        List<RewardTableEntry> empty = new List<RewardTableEntry>();
        Assert.IsFalse(RewardTable.IsValid(empty));
        Assert.IsNull(RewardTable.Roll(empty, 1));
    }
}
