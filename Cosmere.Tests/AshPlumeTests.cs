using System.Collections.Generic;
using Cosmere.System.Scadrial.Grid;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The shape of what a vent puts on the ground. Inverse-square so drifts bank against the
///     vent, wind-skewed so the map develops a downwind side rather than a tidy circle.
/// </summary>
[TestClass]
public class AshPlumeTests {
    [TestMethod]
    public void TheVentCellGetsTheMost() {
        float centre = AshPlume.Weight(0, 0, 1f, 0f, 0f);
        Assert.IsTrue(centre > AshPlume.Weight(1, 0, 1f, 0f, 0f));
        Assert.IsTrue(centre > AshPlume.Weight(0, 1, 1f, 0f, 0f));
    }

    [TestMethod]
    public void WeightFallsOffWithDistance() {
        float near = AshPlume.Weight(2, 0, 1f, 0f, 0f);
        float mid = AshPlume.Weight(5, 0, 1f, 0f, 0f);
        float far = AshPlume.Weight(10, 0, 1f, 0f, 0f);

        Assert.IsTrue(near > mid, $"near {near} should beat mid {mid}");
        Assert.IsTrue(mid > far, $"mid {mid} should beat far {far}");
    }

    [TestMethod]
    public void NothingLandsPastTheRadius() {
        Assert.AreEqual(0f, AshPlume.Weight(AshPlume.RadiusCells + 1, 0, 1f, 0f, 0f), 0.0001f);
        Assert.AreEqual(0f, AshPlume.Weight(0, AshPlume.RadiusCells + 1, 1f, 0f, 0f), 0.0001f);
    }

    [TestMethod]
    public void WithNoSkewThePlumeIsSymmetric() {
        Assert.AreEqual(AshPlume.Weight(4, 0, 1f, 0f, 0f), AshPlume.Weight(-4, 0, 1f, 0f, 0f), 0.0001f);
        Assert.AreEqual(AshPlume.Weight(0, 4, 1f, 0f, 0f), AshPlume.Weight(0, -4, 1f, 0f, 0f), 0.0001f);
    }

    [TestMethod]
    public void SkewFavoursDownwind() {
        // Heading points along +x, so +x is downwind and -x is upwind.
        float downwind = AshPlume.Weight(5, 0, 1f, 0f, 0.6f);
        float upwind = AshPlume.Weight(-5, 0, 1f, 0f, 0.6f);

        Assert.IsTrue(downwind > upwind, $"downwind {downwind} should beat upwind {upwind}");
    }

    [TestMethod]
    public void SkewNeverProducesANegativeOrRunawayWeight() {
        for (int x = -AshPlume.RadiusCells; x <= AshPlume.RadiusCells; x++) {
            for (int z = -AshPlume.RadiusCells; z <= AshPlume.RadiusCells; z++) {
                float w = AshPlume.Weight(x, z, 1f, 0f, 1f);
                Assert.IsTrue(w >= 0f, $"({x},{z}) gave a negative weight {w}");
                Assert.IsTrue(w <= 1f, $"({x},{z}) gave {w}, above the centre weight");
            }
        }
    }

    [TestMethod]
    public void BankDepositsNothingBelowOneUnit() {
        float remainder = 0f;
        int deposit = AshPlume.Bank(ref remainder, 4f, 10);

        Assert.AreEqual(0, deposit);
        Assert.AreEqual(4f, remainder, 0.0001f);
    }

    [TestMethod]
    public void BankEventuallyDepositsAWholeMultipleOfTheUnit() {
        float remainder = 0f;
        int deposit = 0;
        for (int i = 0; i < 4; i++) {
            deposit = AshPlume.Bank(ref remainder, 3f, 10);
        }

        // 4 adds of 3mm = 12mm, which crosses the 10mm unit on the fourth call.
        Assert.AreEqual(10, deposit);
        Assert.AreEqual(0, deposit % 10);
        Assert.AreEqual(2f, remainder, 0.0001f);
    }

    [TestMethod]
    public void BankConservesMassOverManySmallAdds() {
        float remainder = 0f;
        int totalDeposited = 0;
        for (int i = 0; i < 50000; i++) {
            totalDeposited += AshPlume.Bank(ref remainder, 0.15f, 10);
        }

        // 7500mm of adds. Discarding the remainder instead of subtracting compounds slivers under 7480.
        Assert.IsTrue(totalDeposited is >= 7480 and <= 7500, $"deposited {totalDeposited}, expected 7480-7500");
    }

    [TestMethod]
    public void BankIgnoresZeroAndNegativeAdds() {
        float remainder = 5f;

        Assert.AreEqual(0, AshPlume.Bank(ref remainder, 0f, 10));
        Assert.AreEqual(5f, remainder, 0.0001f);

        Assert.AreEqual(0, AshPlume.Bank(ref remainder, -2f, 10));
        Assert.AreEqual(3f, remainder, 0.0001f);
    }

    [TestMethod]
    public void ARestoredBankKeepsEveryFractionItWasSavedWith() {
        float[] saved = new float[3];
        AshPlume.Bank(ref saved[0], 4f, 10);
        AshPlume.Bank(ref saved[1], 9.5f, 10);

        float[]? restored = AshPlume.RestoreBank([..saved], saved.Length);

        Assert.IsNotNull(restored);
        Assert.AreEqual(4f, restored[0], 0.0001f);
        Assert.AreEqual(9.5f, restored[1], 0.0001f);
        Assert.AreEqual(0f, restored[2], 0.0001f);

        // 9.5 was half a millimetre off a deposit; a bank dropped on load restarts the cell at 0.
        Assert.AreEqual(10, AshPlume.Bank(ref restored[1], 0.5f, 10));
    }

    [TestMethod]
    public void ABankSavedAtADifferentRadiusIsDiscardedRatherThanIndexedPast() {
        List<float> saved = [1f, 2f, 3f];

        Assert.IsNull(AshPlume.RestoreBank(saved, 4), "a grown offset list would read past the saved bank");
        Assert.IsNull(AshPlume.RestoreBank(saved, 2), "a shrunk one would leave the tail unread");
        Assert.IsNull(AshPlume.RestoreBank(null, 3), "a save from before the bank existed");
    }

    /// <summary>
    ///     The vent blows its own mouth clear. Every cell of its footprint sits at distance 0, and
    ///     nothing the drift can throw at it may survive there.
    /// </summary>
    [TestMethod]
    public void TheMouthItselfIsAlwaysClear() {
        foreach (int depth in new[] { 0, 10, 900, 1400, AshGrid.MaxDepthMm }) {
            Assert.AreEqual(0, AshPlume.AllowedDepthMm(depth, 0f, 3f), $"{depth}mm survived on the mouth");
        }
    }

    /// <summary>
    ///     A vent thins the drift, it never adds to it. An allowance above the cell's own depth
    ///     would read as the vent piling ash up outside its radius.
    /// </summary>
    [TestMethod]
    public void TheFeatherOnlyEverTakesAway() {
        for (int depth = 0; depth <= AshGrid.MaxDepthMm; depth += 50) {
            for (float distance = 0f; distance <= 4f; distance += 0.25f) {
                int allowed = AshPlume.AllowedDepthMm(depth, distance, 3f);
                Assert.IsTrue(allowed <= depth, $"{distance} cells out, {depth}mm was raised to {allowed}mm");
                Assert.IsTrue(allowed >= 0, $"{distance} cells out, {depth}mm gave a negative {allowed}mm");
            }
        }
    }

    [TestMethod]
    public void PastTheRadiusTheDriftIsLeftAlone() {
        Assert.AreEqual(
            AshPlume.WarmCeilingMm, AshPlume.AllowedDepthMm(2000, 3f, 3f), "the outer ring is the last thinned one"
        );
        Assert.AreEqual(2000, AshPlume.AllowedDepthMm(2000, 3.01f, 3f), "the first ring past the front");
        Assert.AreEqual(2000, AshPlume.AllowedDepthMm(2000, 9f, 3f));
    }

    [TestMethod]
    public void TheAllowanceRisesWithDistance() {
        int previous = -1;
        for (float distance = 0f; distance < 3f; distance += 0.1f) {
            int allowed = AshPlume.AllowedDepthMm(AshGrid.MaxDepthMm, distance, 3f);
            Assert.IsTrue(allowed >= previous, $"the feather dipped at {distance} cells: {allowed} after {previous}");
            previous = allowed;
        }
    }

    /// <summary>
    ///     The comp re-imposes this every sweep cycle. If a second pass moved a cell that the first
    ///     already thinned, the mesh would dirty forever on ground that never actually changes.
    /// </summary>
    [TestMethod]
    public void ThinningAnAlreadyThinnedCellChangesNothing() {
        for (float distance = 0f; distance <= 3f; distance += 0.2f) {
            int once = AshPlume.AllowedDepthMm(AshGrid.MaxDepthMm, distance, 3f);
            int twice = AshPlume.AllowedDepthMm(once, distance, 3f);

            Assert.AreEqual(once, twice, $"{distance} cells out, a second pass moved {once}mm to {twice}mm");
        }
    }

    /// <summary>
    ///     The outer ring has to reach the ceiling, or the vent thins its own far edge harder than
    ///     the ground just inside it and the apron reads inside out.
    /// </summary>
    [TestMethod]
    public void TheOuterRingReachesTheWarmCeiling() {
        int edge = AshPlume.AllowedDepthMm(AshGrid.MaxDepthMm, 8f, 8f);

        Assert.AreEqual(AshPlume.WarmCeilingMm, edge, $"the feather tops out at {edge}mm");
    }

    /// <summary>
    ///     The stall this shipped with: past the clearing nothing was thinned, so the drift crossed
    ///     TerrainSwapMm and the soil ladder stopped around the second rung. Every warmed cell has
    ///     to sit under the restore line, not the swap line - a cell the drift already claimed only
    ///     hands itself back below the lower of the two.
    /// </summary>
    [TestMethod]
    public void WarmedGroundStaysShallowEnoughForTheSoilToClimb() {
        float reach = AshVentSoilSpread.MaxReachCells;

        for (float distance = 0.25f; distance <= reach; distance += 0.25f) {
            int allowed = AshPlume.AllowedDepthMm(AshGrid.MaxDepthMm, distance, reach);

            Assert.IsFalse(
                AshDepthMath.ShouldSwapToAshTerrain(allowed, true),
                $"{distance} cells out the vent allows {allowed}mm, which leaves ash terrain on the cell"
            );
        }
    }

    /// <summary>Turning the feather off in XML must not turn the mouth clearing off with it.</summary>
    [TestMethod]
    public void AZeroRadiusStillClearsTheMouth() {
        Assert.AreEqual(0, AshPlume.AllowedDepthMm(2000, 0f, 0f), "the mouth kept its ash");
        Assert.AreEqual(2000, AshPlume.AllowedDepthMm(2000, 1f, 0f), "a zero radius reached a cell anyway");
    }
}
