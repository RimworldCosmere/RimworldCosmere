using Cosmere.System.Roshar.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards the oath ladder's dot spacing and per-dot state, which decide what a player
///     reads as sworn, current and still ahead.
/// </summary>
[TestClass]
public class OathLadderLayoutTests {
    [TestMethod]
    public void FiveDotsSpaceEvenlyAcrossTheWidth() {
        const float x = 10f;
        const float width = 200f;
        float step = (width - OathLadderLayout.DotSize) / 4f;

        for (int i = 0; i < 5; i++) {
            Assert.AreEqual(x + step * i, OathLadderLayout.DotX(i, 5, x, width), 0.001f);
        }

        Assert.AreEqual(x, OathLadderLayout.DotX(0, 5, x, width), 0.001f);
        Assert.AreEqual(x + width - OathLadderLayout.DotSize, OathLadderLayout.DotX(4, 5, x, width), 0.001f);
    }

    [TestMethod]
    public void ASingleDotSitsAtTheLeftEdge() {
        Assert.AreEqual(10f, OathLadderLayout.DotX(0, 1, 10f, 200f), 0.001f);
    }

    [TestMethod]
    public void ThirdIdealReadsSwornSwornCurrentUnswornUnsworn() {
        OathDotState[] expected = [
            OathDotState.Sworn,
            OathDotState.Sworn,
            OathDotState.Current,
            OathDotState.Unsworn,
            OathDotState.Unsworn,
        ];

        for (int i = 0; i < expected.Length; i++) {
            Assert.AreEqual(expected[i], OathLadderLayout.StateFor(i, 2));
        }
    }

    [TestMethod]
    public void NoOathSwornLeavesEveryDotUnsworn() {
        for (int i = 0; i < 5; i++) {
            Assert.AreEqual(OathDotState.Unsworn, OathLadderLayout.StateFor(i, -1));
        }
    }

    [TestMethod]
    public void PastTheLastIdealMarksEveryDotSworn() {
        for (int i = 0; i < 5; i++) {
            Assert.AreEqual(OathDotState.Sworn, OathLadderLayout.StateFor(i, 5));
        }
    }

    [TestMethod]
    public void CountClampsAtZero() {
        Assert.AreEqual(0, OathLadderLayout.DotCount(0));
        Assert.AreEqual(0, OathLadderLayout.DotCount(-3));
        Assert.AreEqual(5, OathLadderLayout.DotCount(5));
    }
}
