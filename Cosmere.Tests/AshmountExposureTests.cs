using System.Collections.Generic;
using Cosmere.System.Scadrial.World;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Summing every mount rather than taking the nearest is the whole reason the heartland inside
///     the ring is buried. Nearest-only would make the middle the cleanest place on the planet.
/// </summary>
[TestClass]
public class AshmountExposureTests {
    [TestMethod]
    public void AnOrdinaryTileIsExactlyOne() {
        Assert.AreEqual(1f, AshmountExposure.Multiplier([]), 0.0001f);
    }

    [TestMethod]
    public void ContributionFallsOffToNothingAtRange() {
        Assert.AreEqual(0f, AshmountExposure.Contribution(AshmountExposure.RangeTiles), 0.0001f);
        Assert.AreEqual(0f, AshmountExposure.Contribution(AshmountExposure.RangeTiles + 10f), 0.0001f);
        Assert.IsTrue(AshmountExposure.Contribution(0f) > AshmountExposure.Contribution(10f));
        Assert.IsTrue(AshmountExposure.Contribution(10f) > AshmountExposure.Contribution(30f));
    }

    [TestMethod]
    public void AMountOutOfRangeChangesNothing() {
        Assert.AreEqual(1f, AshmountExposure.Multiplier([AshmountExposure.RangeTiles + 1f]), 0.0001f);
    }

    [TestMethod]
    public void TheHeartlandBeatsASingleCloserMount() {
        // Eight mounts at 25 tiles should beat one mount at 20 - that's why summing exists.
        List<float> ring = new List<float>();
        for (int i = 0; i < 8; i++) {
            ring.Add(25f);
        }

        float heartland = AshmountExposure.Multiplier(ring);
        float oneCloserMount = AshmountExposure.Multiplier([20f]);

        Assert.IsTrue(
            heartland > oneCloserMount,
            $"heartland {heartland} should beat a single mount at 20 tiles ({oneCloserMount})"
        );
    }

    [TestMethod]
    public void ExposureIsClampedSoNoTileIsUnplayable() {
        List<float> swarm = new List<float>();
        for (int i = 0; i < 100; i++) {
            swarm.Add(0f);
        }

        Assert.AreEqual(AshmountExposure.MaxMultiplier, AshmountExposure.Multiplier(swarm), 0.0001f);
    }

    [TestMethod]
    public void MoreMountsNeverLowersTheMultiplier() {
        float one = AshmountExposure.Multiplier([15f]);
        float two = AshmountExposure.Multiplier([15f, 30f]);

        Assert.IsTrue(two >= one);
    }
}
