using System.Collections.Generic;
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
    /// <summary>Stripes the tracker sweeps in. Its own constant is private and Verse-bound.</summary>
    private const int Stripes = 64;

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

    /// <summary>
    ///     The sweep drives terrain off this, so a wrong answer either strands ash terrain on a
    ///     drained map or flips the same cell back and forth every 64 ticks.
    /// </summary>
    [TestMethod]
    public void TerrainActionOnlyFiresOnACrossing() {
        Assert.AreEqual(AshTerrainAction.Leave, AshDepthMath.NextTerrainAction(0, false));
        Assert.AreEqual(AshTerrainAction.Leave, AshDepthMath.NextTerrainAction(AshDepthMath.TerrainSwapMm - 1, false));
        Assert.AreEqual(AshTerrainAction.Swap, AshDepthMath.NextTerrainAction(AshDepthMath.TerrainSwapMm, false));

        // Inside the hysteresis band a swapped cell has nothing to do, either way.
        Assert.AreEqual(AshTerrainAction.Leave, AshDepthMath.NextTerrainAction(AshDepthMath.TerrainSwapMm, true));
        Assert.AreEqual(
            AshTerrainAction.Leave, AshDepthMath.NextTerrainAction(AshDepthMath.TerrainRestoreMm + 1, true)
        );

        Assert.AreEqual(AshTerrainAction.Restore, AshDepthMath.NextTerrainAction(AshDepthMath.TerrainRestoreMm, true));
        Assert.AreEqual(AshTerrainAction.Restore, AshDepthMath.NextTerrainAction(0, true));
    }

    /// <summary>A drained cell must always come back, or the Catacendre leaves ash terrain behind.</summary>
    [TestMethod]
    public void EveryDepthSettlesOnOneAnswer() {
        for (int mm = 0; mm <= AshGrid.MaxDepthMm; mm += 10) {
            AshTerrainAction swapped = AshDepthMath.NextTerrainAction(mm, true);
            AshTerrainAction clean = AshDepthMath.NextTerrainAction(mm, false);

            Assert.AreNotEqual(AshTerrainAction.Restore, clean, $"{mm}mm asks an unswapped cell to restore");
            Assert.AreNotEqual(AshTerrainAction.Swap, swapped, $"{mm}mm asks a swapped cell to swap again");
        }

        Assert.AreEqual(AshTerrainAction.Restore, AshDepthMath.NextTerrainAction(0, true));
        Assert.IsTrue(AshDepthMath.TerrainChangesPerSweep > 0, "a zero budget stalls the sweep forever");
    }

    /// <summary>
    ///     The dwell is what keeps the map from turning over as one wave. A banded or clamped
    ///     hash would do it in stripes, which is the thing this replaced.
    /// </summary>
    [TestMethod]
    public void SettleDelayStaysInRangeAndSpreadsAcrossIt() {
        foreach (bool settling in new[] { true, false }) {
            int[] histogram = new int[AshDepthMath.SettleMaxDays + 1];

            for (int i = 0; i < 62500; i++) {
                int days = AshDepthMath.SettleDelayDays(i, settling);
                Assert.IsTrue(
                    days >= AshDepthMath.SettleMinDays && days <= AshDepthMath.SettleMaxDays,
                    $"cell {i} waits {days} days, outside {AshDepthMath.SettleMinDays} to {AshDepthMath.SettleMaxDays}"
                );

                histogram[days]++;
            }

            // Roughly even, or a whole bucket of cells flips on one day and the wave is back.
            int span = AshDepthMath.SettleMaxDays - AshDepthMath.SettleMinDays + 1;
            int expected = 62500 / span;
            for (int days = AshDepthMath.SettleMinDays; days <= AshDepthMath.SettleMaxDays; days++) {
                Assert.IsTrue(
                    histogram[days] > expected / 2 && histogram[days] < expected * 2,
                    $"settling={settling}: {days} days got {histogram[days]} against an even share of {expected}"
                );
            }
        }
    }

    /// <summary>Neighbours sharing a delay is what banding looks like before it reaches the map.</summary>
    [TestMethod]
    public void AdjacentCellsRarelyShareADelay() {
        int matches = 0;
        for (int i = 0; i < 62500 - 1; i++) {
            if (AshDepthMath.SettleDelayDays(i, true) == AshDepthMath.SettleDelayDays(i + 1, true)) matches++;
        }

        // One in 24 by chance across 24 possible delays; anything near that is a healthy hash.
        Assert.IsTrue(matches < 62500 / 8, $"{matches} of 62,499 neighbouring pairs settle on the same day");
    }

    /// <summary>
    ///     The two directions are salted apart, so the ground does not come back in the same
    ///     pattern it went under - which would read as the whole thing running in reverse.
    /// </summary>
    [TestMethod]
    public void SettlingAndRevertingUseDifferentDelays() {
        int matches = 0;
        for (int i = 0; i < 62500; i++) {
            if (AshDepthMath.SettleDelayDays(i, true) == AshDepthMath.SettleDelayDays(i, false)) matches++;
        }

        Assert.IsTrue(matches < 62500 / 8, $"{matches} of 62,500 cells revert on the same schedule they settled on");
    }

    /// <summary>The delay is derived, not rolled, so a reload cannot shake out a different map.</summary>
    [TestMethod]
    public void SettleDelayIsStableForACell() {
        for (int i = 0; i < 1000; i++) {
            Assert.AreEqual(AshDepthMath.SettleDelayDays(i, true), AshDepthMath.SettleDelayDays(i, true));
            Assert.AreEqual(AshDepthMath.SettleDelayDays(i, false), AshDepthMath.SettleDelayDays(i, false));
        }
    }

    /// <summary>
    ///     The drain used to strip 30mm per stripe visit, which emptied a fully buried map in
    ///     about two hours. That left deep-ash terrain sitting on ground with no ash on it for
    ///     weeks while the revert dwell ran down.
    /// </summary>
    [TestMethod]
    public void TheCatacendreDrainTakesAboutThreeDays() {
        const float SweepsPerDay = 60000f / 64f;

        float perSweep = AshDepthMath.DrainMmPerSweep(AshGrid.MaxDepthMm, SweepsPerDay);
        Assert.IsTrue(perSweep > 0f, "a zero drain never clears the map");

        float depth = AshGrid.MaxDepthMm;
        int sweeps = 0;
        while (depth > 0f && sweeps < 1_000_000) {
            depth -= perSweep;
            sweeps++;
        }

        float days = sweeps / SweepsPerDay;
        Assert.AreEqual(AshDepthMath.DrainDays, days, 0.05f, $"a capped cell took {days} days to clear");
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
        float stepped = AshDepthMath.EaseSeverity(0f, 1f, AshDepthMath.SeverityEasePerDay, oneTick);

        Assert.IsTrue(stepped > 0f);
        Assert.IsTrue(stepped < 0.001f, $"one tick moved severity by {stepped}, which reads as a jump cut");
    }

    [TestMethod]
    public void SeverityEventuallyArrives() {
        float value = 0f;
        for (int day = 0; day < 200; day++) {
            value = AshDepthMath.EaseSeverity(value, 1f, AshDepthMath.SeverityEasePerDay, 1f);
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

    [TestMethod]
    public void AStripeBankKeepsEveryFractionAcrossTheRoundTrip() {
        float[] accrual = new float[Stripes];
        AshPlume.Bank(ref accrual[0], 9.5f, AshGrid.UnitMm);
        AshPlume.Bank(ref accrual[Stripes - 1], 4f, AshGrid.UnitMm);

        float[]? restored = AshPlume.RestoreBank([..accrual], Stripes);

        Assert.IsNotNull(restored);
        Assert.AreEqual(9.5f, restored[0], 0.0001f);
        Assert.AreEqual(4f, restored[Stripes - 1], 0.0001f);

        // Stripe 0 sat half a millimetre off depositing; a bank dropped on load restarts it at zero.
        Assert.AreEqual(AshGrid.UnitMm, AshPlume.Bank(ref restored[0], 0.5f, AshGrid.UnitMm));
    }

    [TestMethod]
    public void AStripeBankSavedAtADifferentWidthIsDiscardedRatherThanIndexedPast() {
        Assert.IsNull(AshPlume.RestoreBank(new List<float>(new float[Stripes - 1]), Stripes));
        Assert.IsNull(AshPlume.RestoreBank(new List<float>(new float[Stripes + 1]), Stripes));
        Assert.IsNull(AshPlume.RestoreBank(null, Stripes), "a save from before the accrual was scribed");
    }
}
