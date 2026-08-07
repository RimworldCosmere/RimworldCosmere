using Cosmere.System.Scadrial.Grid;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

[TestClass]
public class AshBuriedCellsTests {
    [TestMethod]
    public void ACellCrossesTheBurialLineOnlyAtNineHundred() {
        AshBuriedCells cells = new AshBuriedCells(16);
        Assert.IsFalse(AshDepthMath.IsBuried(899, cells.IsBuried(3)));
        Assert.IsTrue(AshDepthMath.IsBuried(900, cells.IsBuried(3)));
    }

    [TestMethod]
    public void OnceBuriedACellStaysBuriedUntilThreeHundred() {
        AshBuriedCells cells = new AshBuriedCells(16);
        cells.Set(3, true);
        Assert.IsTrue(AshDepthMath.IsBuried(301, cells.IsBuried(3)));
        Assert.IsFalse(AshDepthMath.IsBuried(300, cells.IsBuried(3)));
        Assert.IsFalse(AshDepthMath.IsBuried(0, cells.IsBuried(3)), "ash gone means the cell unburies");
    }

    [TestMethod]
    public void SetReportsOnlyRealChanges() {
        AshBuriedCells cells = new AshBuriedCells(16);
        Assert.IsTrue(cells.Set(5, true));
        Assert.IsFalse(cells.Set(5, true));
        Assert.IsTrue(cells.Set(5, false));
    }

    [TestMethod]
    public void CountTracksTheNumberOfBuriedCells() {
        AshBuriedCells cells = new AshBuriedCells(16);
        cells.Set(1, true);
        cells.Set(2, true);
        cells.Set(1, false);
        Assert.AreEqual(1, cells.Count);
        Assert.IsTrue(cells.Any);
    }

    [TestMethod]
    public void ClearingTheSetUnburiesEveryCell() {
        AshBuriedCells cells = new AshBuriedCells(16);
        cells.Set(1, true);
        cells.Set(9, true);

        cells.Clear();

        Assert.AreEqual(0, cells.Count);
        Assert.IsFalse(cells.Any);
        Assert.IsFalse(cells.IsBuried(1));
        Assert.IsFalse(cells.IsBuried(9));
    }

    [TestMethod]
    public void SeedingFromDepthUsesTheBurialLineAndNotTheHysteresisFloor() {
        AshBuriedCells cells = new AshBuriedCells(5);
        int[] depth = [0, 500, 890, 900, 2550];

        cells.SeedFromDepth(i => depth[i]);

        Assert.IsFalse(cells.IsBuried(0));
        Assert.IsFalse(cells.IsBuried(1));
        Assert.IsFalse(cells.IsBuried(2));
        Assert.IsTrue(cells.IsBuried(3));
        Assert.IsTrue(cells.IsBuried(4));
        Assert.AreEqual(2, cells.Count);
    }

    [TestMethod]
    public void AnIndexOutsideTheGridIsNeverBuried() {
        AshBuriedCells cells = new AshBuriedCells(16);
        Assert.IsFalse(cells.IsBuried(-1));
        Assert.IsFalse(cells.IsBuried(16));
        Assert.IsFalse(cells.Set(99, true));
    }

    [TestMethod]
    public void ScribedBuriedCellsSurviveARoundTrip() {
        AshBuriedCells before = new AshBuriedCells(64);
        before.Set(7, true);
        before.Set(31, true);

        AshBuriedCells after = AshBuriedCells.Restore(before.Raw, 64);

        Assert.IsTrue(after.IsBuried(7));
        Assert.IsTrue(after.IsBuried(31));
        Assert.IsFalse(after.IsBuried(8));
        Assert.AreEqual(2, after.Count);
    }

    [TestMethod]
    public void ASavedSetOfTheWrongLengthIsDiscardedNotIndexed() {
        bool[] stale = new bool[16];
        stale[3] = true;

        AshBuriedCells restored = AshBuriedCells.Restore(stale, 64);

        Assert.AreEqual(0, restored.Count);
        Assert.IsFalse(restored.IsBuried(3));
    }
}
