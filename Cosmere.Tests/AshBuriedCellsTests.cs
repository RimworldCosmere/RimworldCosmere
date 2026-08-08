using System;
using System.IO;
using Cosmere.System.Scadrial.Grid;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

[TestClass]
public class AshBuriedCellsTests {
    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereScadrial/Defs above the test output directory.");
            return dir!.FullName;
        }
    }

    /// <summary>
    ///     Five callers have now moved depth and left the set stale, and each one shipped looking
    ///     correct. Nothing runtime can catch it, so the rule is enforced against the source.
    /// </summary>
    [TestMethod]
    public void EveryDepthWriterOutsideTheSweepAlsoTouchesTheBuriedSet() {
        string root = Path.Combine(RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial");
        int checked_ = 0;

        foreach (string path in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories)) {
            string name = Path.GetFileName(path);

            // AshGrid declares the mutators. AshDepthTracker owns the sweep that reconciles the set.
            if (name is "AshGrid.cs" or "AshDepthTracker.cs") continue;

            string source = File.ReadAllText(path);
            if (!source.Contains("AddDepthMm(") && !source.Contains("RemoveDepthMm(")) continue;

            checked_++;
            Assert.IsTrue(
                source.Contains("Buried"),
                $"{name} moves ash depth and never mentions the buried set. The wash and the haul " +
                "block both read that set, so every cell it touches goes stale until the sweep " +
                "reaches its stripe - and if it dirties the mesh itself, the player sees the stale frame."
            );
        }

        Assert.IsTrue(checked_ > 0, "found no depth writers at all, so this guard is not actually looking at anything");
    }

    [TestMethod]
    public void ACellCrossesTheBurialLineOnlyAtNineHundred() {
        AshBuriedCells cells = new AshBuriedCells(16);
        Assert.IsFalse(AshDepthMath.IsBuried(899, cells.IsBuried(3)));
        Assert.IsTrue(AshDepthMath.IsBuried(900, cells.IsBuried(3)));
    }

    [TestMethod]
    public void OnceBuriedACellStaysBuriedDownToTheUncoveredLine() {
        AshBuriedCells cells = new AshBuriedCells(16);
        cells.Set(3, true);
        Assert.IsTrue(AshDepthMath.IsBuried(AshDepthMath.UncoveredMm + 1, cells.IsBuried(3)));
        Assert.IsFalse(AshDepthMath.IsBuried(AshDepthMath.UncoveredMm, cells.IsBuried(3)));
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
    public void RefreshingFromDepthKeepsTheHysteresisThatSeedingDiscards() {
        AshBuriedCells cells = new AshBuriedCells(4);
        int between = AshDepthMath.UncoveredMm + 1;
        int[] depth = [between, between, AshDepthMath.BuriedMm, 0];
        cells.Set(0, true);
        cells.Set(3, true);

        cells.RefreshFromDepth(i => depth[i]);

        Assert.IsTrue(cells.IsBuried(0), "a buried cell above the unbury line stays buried");
        Assert.IsFalse(cells.IsBuried(1), "an unburied cell needs the full burial line");
        Assert.IsTrue(cells.IsBuried(2));
        Assert.IsFalse(cells.IsBuried(3), "ash gone means the cell unburies");
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
