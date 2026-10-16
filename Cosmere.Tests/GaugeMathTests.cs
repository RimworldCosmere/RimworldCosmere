using Cosmere.Core.UI.Dock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Covers where a dragged refill threshold lands on the dock gauge.
/// </summary>
/// <remarks>
///     Only the arithmetic is testable. TargetGauge.Draw is IMGUI all the way down - it
///     needs Event.current, a live GUI and an on-screen Rect - so it is verified in game
///     rather than here.
/// </remarks>
[TestClass]
public class GaugeMathTests {
    [TestMethod]
    public void DraggingLeftOfTheTrackClampsToZero() {
        Assert.AreEqual(0f, GaugeMath.SnapTarget(-0.4f, 700f), 0.001f);
    }

    [TestMethod]
    public void DraggingRightOfTheTrackClampsToMax() {
        Assert.AreEqual(700f, GaugeMath.SnapTarget(1.8f, 700f), 0.001f);
    }

    [TestMethod]
    public void TargetSnapsToFivePercentSteps() {
        // 0.42 sits between the 40% and 45% stops and takes the nearer one.
        Assert.AreEqual(280f, GaugeMath.SnapTarget(0.42f, 700f), 0.001f);
        Assert.AreEqual(315f, GaugeMath.SnapTarget(0.44f, 700f), 0.001f);
    }

    [TestMethod]
    public void EverySnappedTargetLandsOnAWholeStep() {
        for (int i = 0; i <= 100; i++) {
            float snapped = GaugeMath.SnapTarget(i / 100f, 3000f);
            float steps = snapped / (3000f * GaugeMath.SnapStep);

            Assert.AreEqual(
                steps,
                (float)global::System.Math.Round(steps),
                0.001f,
                $"Dragging to {i}% produced {snapped}, which is not on a {GaugeMath.SnapStep:P0} step."
            );
        }
    }

    /// <summary>
    ///     A Radiant whose capacity has not resolved yet must not produce a NaN threshold,
    ///     because the section writes the result straight back onto the gene and it is saved.
    /// </summary>
    [TestMethod]
    public void ZeroMaxProducesZeroRatherThanNaN() {
        float snapped = GaugeMath.SnapTarget(0.5f, 0f);

        Assert.AreEqual(0f, snapped, 0.001f);
        Assert.IsFalse(float.IsNaN(snapped));
    }
}
