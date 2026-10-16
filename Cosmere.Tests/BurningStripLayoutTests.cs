using Cosmere.System.Scadrial.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Heights for the pinned strip that lists whatever the pawn is burning.
/// </summary>
[TestClass]
public class BurningStripLayoutTests {
    [TestMethod]
    public void NothingBurningCostsNoHeight() {
        Assert.AreEqual(0f, BurningStripLayout.HeightFor(0), 0.001f);
        Assert.AreEqual(0f, BurningStripLayout.HeightFor(-1), 0.001f);
    }

    [TestMethod]
    public void RowsCarryTheHeaderAndOneGap() {
        Assert.AreEqual(
            BurningStripLayout.HeaderHeight + BurningStripLayout.Padding + BurningStripLayout.RowHeight + BurningStripLayout.Footer,
            BurningStripLayout.HeightFor(1),
            0.001f
        );

        Assert.AreEqual(
            BurningStripLayout.HeaderHeight + BurningStripLayout.Padding + BurningStripLayout.RowHeight * 4f + BurningStripLayout.Footer,
            BurningStripLayout.HeightFor(4),
            0.001f
        );
    }
}
