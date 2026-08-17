using Cosmere.Core.ShardConnection;
using Cosmere.System.Scadrial.Feruchemy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     How a duralumin transfer spreads across a pawn's non-Shard SpiritWeb edges: proportional to
///     value on the way out, proportional to headroom on the way back in.
/// </summary>
[TestClass]
public class BondDistributionTests {
    private const float Tolerance = 1e-3f;

    private static float Sum(float[] values) {
        float total = 0f;

        for (int i = 0; i < values.Length; i++) total += values[i];

        return total;
    }

    [TestMethod]
    public void DrainIsProportionalToValue() {
        (float moved, float[] deltas) = BondDistribution.Drain(new[] { 0.8f, 0.4f, 0.0f }, 60f);

        Assert.AreEqual(60f, moved, Tolerance);
        Assert.AreEqual(-0.4f, deltas[0], Tolerance);
        Assert.AreEqual(-0.2f, deltas[1], Tolerance);
        Assert.AreEqual(0f, deltas[2], Tolerance);
        Assert.AreEqual(deltas[0], deltas[1] * 2f, Tolerance, "0.8 gives twice what 0.4 gives.");
    }

    [TestMethod]
    public void RestoreIsProportionalToHeadroom() {
        (float moved, float[] deltas) = BondDistribution.Restore(new[] { 0.8f, 0.4f, 1.0f }, 40f);

        Assert.AreEqual(40f, moved, Tolerance);
        Assert.AreEqual(0.1f, deltas[0], Tolerance);
        Assert.AreEqual(0.3f, deltas[1], Tolerance);
        Assert.AreEqual(0f, deltas[2], Tolerance);
        Assert.AreEqual(deltas[1], deltas[0] * 3f, Tolerance, "0.4's headroom is three times 0.8's.");
    }

    [TestMethod]
    public void DrainDeltasSumToWhatWasReported() {
        (float moved, float[] deltas) = BondDistribution.Drain(new[] { 0.8f, 0.4f, 0.0f }, 60f);

        Assert.AreEqual(moved, -Sum(deltas) * ConnectionMath.Max, Tolerance);
    }

    [TestMethod]
    public void RestoreDeltasSumToWhatWasReported() {
        (float moved, float[] deltas) = BondDistribution.Restore(new[] { 0.8f, 0.4f, 1.0f }, 40f);

        Assert.AreEqual(moved, Sum(deltas) * ConnectionMath.Max, Tolerance);
    }

    [TestMethod]
    public void DrainPastTheTotalAvailableMovesOnlyWhatExists() {
        (float moved, float[] deltas) = BondDistribution.Drain(new[] { 0.3f, 0.2f }, 1000f);

        Assert.AreEqual(50f, moved, Tolerance, "0.3 + 0.2 is 50 points' worth, not the 1000 asked.");
        Assert.AreEqual(0f, 0.3f + deltas[0], Tolerance);
        Assert.AreEqual(0f, 0.2f + deltas[1], Tolerance);
    }

    [TestMethod]
    public void RestorePastTheTotalHeadroomMovesOnlyWhatFits() {
        (float moved, float[] deltas) = BondDistribution.Restore(new[] { 0.9f, 0.95f }, 1000f);

        Assert.AreEqual(15f, moved, Tolerance, "0.1 + 0.05 headroom is 15 points' worth, not the 1000 asked.");
        Assert.AreEqual(1f, 0.9f + deltas[0], Tolerance);
        Assert.AreEqual(1f, 0.95f + deltas[1], Tolerance);
    }

    [TestMethod]
    public void DrainFromAPoolAbove100PointsMovesThePoolNotACapAt100() {
        (float moved, float[] deltas) = BondDistribution.Drain(new[] { 0.9f, 0.9f, 0.9f }, 300f);

        Assert.AreEqual(270f, moved, Tolerance, "3 edges at 0.9 is 270 points available, not a 100-point cap.");
        Assert.AreEqual(-0.9f, deltas[0], Tolerance);
        Assert.AreEqual(-0.9f, deltas[1], Tolerance);
        Assert.AreEqual(-0.9f, deltas[2], Tolerance);
    }

    [TestMethod]
    public void RestoreToAPoolAbove100PointsMovesTheHeadroomNotACapAt100() {
        (float moved, float[] deltas) = BondDistribution.Restore(new[] { 0.1f, 0.1f, 0.1f }, 300f);

        Assert.AreEqual(270f, moved, Tolerance, "3 edges with 0.9 headroom each is 270 points, not a 100-point cap.");
        Assert.AreEqual(0.9f, deltas[0], Tolerance);
        Assert.AreEqual(0.9f, deltas[1], Tolerance);
        Assert.AreEqual(0.9f, deltas[2], Tolerance);
    }

    [TestMethod]
    public void NoEdgeMovesOutsideItsRangeEvenWhenTheAskIsHuge() {
        (_, float[] drainDeltas) = BondDistribution.Drain(new[] { 0.3f, 0.2f }, 1000f);
        Assert.IsTrue(0.3f + drainDeltas[0] >= 0f);
        Assert.IsTrue(0.2f + drainDeltas[1] >= 0f);

        (_, float[] restoreDeltas) = BondDistribution.Restore(new[] { 0.9f, 0.95f }, 1000f);
        Assert.IsTrue(0.9f + restoreDeltas[0] <= 1f);
        Assert.IsTrue(0.95f + restoreDeltas[1] <= 1f);
    }

    [TestMethod]
    public void NoEdgesMovesNothing() {
        (float moved, float[] deltas) = BondDistribution.Drain(new float[0], 50f);
        Assert.AreEqual(0f, moved);
        Assert.AreEqual(0, deltas.Length);
    }

    [TestMethod]
    public void EveryEdgeAtZeroCannotBeDrained() {
        (float moved, float[] deltas) = BondDistribution.Drain(new[] { 0f, 0f, 0f }, 50f);
        Assert.AreEqual(0f, moved);
        for (int i = 0; i < deltas.Length; i++) Assert.AreEqual(0f, deltas[i]);
    }

    [TestMethod]
    public void EveryEdgeAtOneCannotBeRestored() {
        (float moved, float[] deltas) = BondDistribution.Restore(new[] { 1f, 1f }, 50f);
        Assert.AreEqual(0f, moved);
        for (int i = 0; i < deltas.Length; i++) Assert.AreEqual(0f, deltas[i]);
    }

    [TestMethod]
    public void NegativeOrZeroAskMovesNothing() {
        float[] edges = { 0.5f, 0.5f };

        (float drainAtZero, _) = BondDistribution.Drain(edges, 0f);
        (float drainNegative, _) = BondDistribution.Drain(edges, -10f);
        (float restoreAtZero, _) = BondDistribution.Restore(edges, 0f);
        (float restoreNegative, _) = BondDistribution.Restore(edges, -10f);

        Assert.AreEqual(0f, drainAtZero);
        Assert.AreEqual(0f, drainNegative);
        Assert.AreEqual(0f, restoreAtZero);
        Assert.AreEqual(0f, restoreNegative);
    }

    [TestMethod]
    public void RoundTripConservesTheTotalButNotTheDistribution() {
        float[] start = { 0.9f, 0.1f };

        (float drained, float[] drainDeltas) = BondDistribution.Drain(start, 50f);
        float[] afterDrain = { start[0] + drainDeltas[0], start[1] + drainDeltas[1] };

        (float restored, float[] restoreDeltas) = BondDistribution.Restore(afterDrain, 50f);
        float[] final = { afterDrain[0] + restoreDeltas[0], afterDrain[1] + restoreDeltas[1] };

        Assert.AreEqual(drained, restored, Tolerance, "The same amount left and came back.");
        Assert.AreEqual(Sum(start), Sum(final), Tolerance, "The total is conserved.");
        Assert.AreNotEqual(start[0], final[0], 0.01f, "The distribution moved: it came back spread by headroom, not by the original split.");
    }
}
