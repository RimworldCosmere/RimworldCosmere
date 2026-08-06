using System;
using System.Collections.Generic;
using Cosmere.System.Scadrial.World;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The ring is a world-generation decision the player can never undo, so its shape is worth
///     pinning: inside the annulus, spaced apart, hills preferred, and stable for a seed.
/// </summary>
[TestClass]
public class AshmountRingTests {
    /// <summary>Tiles laid on a line, so distance between two is just the id difference.</summary>
    private static float LineDistance(int a, int b) {
        return Math.Abs(a - b);
    }

    private static List<AshmountRing.Candidate> Line(int count, bool hilly = true) {
        List<AshmountRing.Candidate> candidates = new List<AshmountRing.Candidate>();
        for (int i = 0; i < count; i++) {
            candidates.Add(new AshmountRing.Candidate(i, i, hilly));
        }

        return candidates;
    }

    [TestMethod]
    public void EveryChosenTileLandsInsideTheAnnulus() {
        List<int> chosen = AshmountRing.Choose(Line(100), 20f, 40f, 8, 1f, LineDistance, 1);

        Assert.IsTrue(chosen.Count > 0, "the ring came out empty");
        for (int i = 0; i < chosen.Count; i++) {
            Assert.IsTrue(chosen[i] >= 20 && chosen[i] <= 40, $"tile {chosen[i]} is outside the band");
        }
    }

    [TestMethod]
    public void SpacingIsHeld() {
        List<int> chosen = AshmountRing.Choose(Line(200), 0f, 200f, 20, 10f, LineDistance, 7);

        for (int i = 0; i < chosen.Count; i++) {
            for (int j = i + 1; j < chosen.Count; j++) {
                Assert.IsTrue(
                    LineDistance(chosen[i], chosen[j]) >= 10f,
                    $"{chosen[i]} and {chosen[j]} are closer than the minimum spacing"
                );
            }
        }
    }

    [TestMethod]
    public void HillsWinWhenBothAreAvailable() {
        List<AshmountRing.Candidate> mixed = new List<AshmountRing.Candidate>();
        for (int i = 0; i < 40; i++) {
            mixed.Add(new AshmountRing.Candidate(i, i, i % 2 == 0));
        }

        List<int> chosen = AshmountRing.Choose(mixed, 0f, 40f, 10, 1f, LineDistance, 3);

        for (int i = 0; i < chosen.Count; i++) {
            Assert.AreEqual(0, chosen[i] % 2, $"tile {chosen[i]} is flat and a hilly one was free");
        }
    }

    [TestMethod]
    public void SameSeedGivesTheSameRing() {
        List<int> first = AshmountRing.Choose(Line(120), 10f, 60f, 12, 3f, LineDistance, 42);
        List<int> second = AshmountRing.Choose(Line(120), 10f, 60f, 12, 3f, LineDistance, 42);

        CollectionAssert.AreEqual(first, second);
    }

    [TestMethod]
    public void DifferentSeedsGiveDifferentRings() {
        List<int> first = AshmountRing.Choose(Line(120), 10f, 60f, 12, 3f, LineDistance, 1);
        List<int> second = AshmountRing.Choose(Line(120), 10f, 60f, 12, 3f, LineDistance, 2);

        CollectionAssert.AreNotEqual(first, second);
    }

    [TestMethod]
    public void ABandTooTightToHoldThemAllReturnsFewerRatherThanHanging() {
        List<int> chosen = AshmountRing.Choose(Line(100), 20f, 30f, 50, 5f, LineDistance, 5);

        Assert.IsTrue(chosen.Count > 0);
        Assert.IsTrue(chosen.Count < 50, "the band cannot hold fifty mounts five apart");
    }

    [TestMethod]
    public void NoCandidatesGivesAnEmptyRingRatherThanThrowing() {
        List<int> chosen = AshmountRing.Choose([], 10f, 20f, 5, 2f, LineDistance, 1);

        Assert.AreEqual(0, chosen.Count);
    }
}
