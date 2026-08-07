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
    public void AnIndexOutsideTheGridIsNeverBuried() {
        AshBuriedCells cells = new AshBuriedCells(16);
        Assert.IsFalse(cells.IsBuried(-1));
        Assert.IsFalse(cells.IsBuried(16));
        Assert.IsFalse(cells.Set(99, true));
    }
}
