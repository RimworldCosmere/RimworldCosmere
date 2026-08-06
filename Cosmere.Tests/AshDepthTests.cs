using Cosmere.System.Scadrial.Grid;
using Cosmere.System.Scadrial.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Covers the ash depth maths and the era gate. Both are deliberately kept free of Verse so
///     they can run here; AshGrid and AshDepthTracker themselves need a live Map and cannot.
/// </summary>
[TestClass]
public class AshDepthTests {
    [TestMethod]
    public void BucketsRiseWithDepth() {
        Assert.AreEqual(AshBucket.None, AshDepthMath.BucketFor(0));
        Assert.AreEqual(AshBucket.None, AshDepthMath.BucketFor(AshDepthMath.DustingMm - 1));
        Assert.AreEqual(AshBucket.Dusting, AshDepthMath.BucketFor(AshDepthMath.DustingMm));
        Assert.AreEqual(AshBucket.Ankle, AshDepthMath.BucketFor(AshDepthMath.AnkleMm));
        Assert.AreEqual(AshBucket.Knee, AshDepthMath.BucketFor(AshDepthMath.KneeMm));
        Assert.AreEqual(AshBucket.Waist, AshDepthMath.BucketFor(AshDepthMath.WaistMm));
        Assert.AreEqual(AshBucket.Waist, AshDepthMath.BucketFor(AshGrid.MaxDepthMm));
    }

    [TestMethod]
    public void MovementPenaltyOnlyBitesPastADusting() {
        Assert.AreEqual(0, AshDepthMath.MovementTicksAddOn(AshBucket.None));
        Assert.AreEqual(0, AshDepthMath.MovementTicksAddOn(AshBucket.Dusting));
        Assert.IsTrue(AshDepthMath.MovementTicksAddOn(AshBucket.Ankle) > 0);
        Assert.IsTrue(
            AshDepthMath.MovementTicksAddOn(AshBucket.Waist) > AshDepthMath.MovementTicksAddOn(AshBucket.Knee)
        );
    }

    [TestMethod]
    public void TerrainSwapHasHysteresisSoDriftingCellsDoNotFlicker() {
        Assert.IsFalse(AshDepthMath.ShouldSwapToAshTerrain(AshDepthMath.TerrainSwapMm - 1, false));
        Assert.IsTrue(AshDepthMath.ShouldSwapToAshTerrain(AshDepthMath.TerrainSwapMm, false));

        // Already swapped: it takes a much bigger drop to give the terrain back.
        Assert.IsTrue(AshDepthMath.ShouldSwapToAshTerrain(AshDepthMath.TerrainSwapMm - 1, true));
        Assert.IsTrue(AshDepthMath.ShouldSwapToAshTerrain(AshDepthMath.TerrainRestoreMm + 1, true));
        Assert.IsFalse(AshDepthMath.ShouldSwapToAshTerrain(AshDepthMath.TerrainRestoreMm, true));
    }

    [TestMethod]
    public void BurialHasHysteresisToo() {
        Assert.IsFalse(AshDepthMath.IsBuried(AshDepthMath.BuriedMm - 1, false));
        Assert.IsTrue(AshDepthMath.IsBuried(AshDepthMath.BuriedMm, false));
        Assert.IsTrue(AshDepthMath.IsBuried(AshDepthMath.UncoveredMm + 1, true));
        Assert.IsFalse(AshDepthMath.IsBuried(AshDepthMath.UncoveredMm, true));
    }

    [TestMethod]
    public void FallRateIsZeroAtRestAndRisesWithSeverity() {
        Assert.AreEqual(0f, AshDepthMath.FallRateMmPerHour(0f, 300f), 0.0001f);

        float low = AshDepthMath.FallRateMmPerHour(0.3f, 300f);
        float high = AshDepthMath.FallRateMmPerHour(0.9f, 300f);
        Assert.IsTrue(high > low);
        Assert.AreEqual(300f / 24f, AshDepthMath.FallRateMmPerHour(1f, 300f), 0.0001f);
    }

    [TestMethod]
    public void SeverityNeverJumpsToItsTarget() {
        float oneTick = 1f / 60000f;
        float stepped = AshDepthMath.EaseSeverity(0f, 1f, 0.12f, oneTick);

        Assert.IsTrue(stepped > 0f);
        Assert.IsTrue(stepped < 0.001f, $"one tick moved severity by {stepped}, which reads as a jump cut");
    }

    [TestMethod]
    public void SeverityEventuallyArrives() {
        float value = 0f;
        for (int day = 0; day < 200; day++) {
            value = AshDepthMath.EaseSeverity(value, 1f, 0.12f, 1f);
        }

        Assert.AreEqual(1f, value, 0.0001f);
    }

    [TestMethod]
    public void OnlyThePreCatacendreEraGetsAsh() {
        Assert.IsTrue(AshEra.IsAshEra("Cosmere_Scadrial_Era_PreCatacendre"));

        string[] ashFree = [
            "Cosmere_Scadrial_Era_PostCatacendre",
            "Cosmere_Scadrial_Era_AlloyOfLaw",
            "Cosmere_Scadrial_Era_ShadowsOfSelf",
            "Cosmere_Scadrial_Era_BandsOfMourning",
            "Cosmere_Scadrial_Era_LostMetal",
        ];

        for (int i = 0; i < ashFree.Length; i++) {
            Assert.IsFalse(AshEra.IsAshEra(ashFree[i]), $"{ashFree[i]} must never see ash");
        }

        Assert.IsFalse(AshEra.IsAshEra(null));
        Assert.IsFalse(AshEra.IsAshEra(string.Empty));
    }
}
