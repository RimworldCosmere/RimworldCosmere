using System;
using System.IO;
using Cosmere.Core.UI.Dock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The dock body budget is the one number standing between a long pinned strip and a scroll
///     view drawn at negative height, which stacks the next section's header on top of the strip.
/// </summary>
[TestClass]
public class DockBodyBudgetTests {
    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereCore", "Languages"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereCore/Languages above the test output directory.");

            return dir.FullName;
        }
    }

    private static string Dock(string file) => File.ReadAllText(Path.Combine(
        RepoRoot, "CosmereCore", "CosmereCore", "Core", "UI", "Dock", file
    ));

    [TestMethod]
    public void PinnedBiggerThanTheSettingLeavesNothing() {
        Assert.AreEqual(0f, DockBodyBudget.For(400f, 200f, 260f), 0.001f);
    }

    [TestMethod]
    public void PinnedExactlyFillingTheSettingLeavesNothing() {
        Assert.AreEqual(0f, DockBodyBudget.For(400f, 200f, 200f), 0.001f);
    }

    [TestMethod]
    public void NoRoomLeftOnScreenLeavesNothing() {
        Assert.AreEqual(0f, DockBodyBudget.For(-30f, 400f, 0f), 0.001f);
    }

    [TestMethod]
    public void TheSmallerCeilingWins() {
        Assert.AreEqual(120f, DockBodyBudget.For(120f, 400f, 50f), 0.001f);
        Assert.AreEqual(350f, DockBodyBudget.For(600f, 400f, 50f), 0.001f);
    }

    [TestMethod]
    public void WithoutAPinnedStripTheSettingIsTheCeiling() {
        Assert.AreEqual(400f, DockBodyBudget.For(900f, 400f, 0f), 0.001f);
        Assert.AreEqual(90f, DockBodyBudget.For(90f, 400f, 0f), 0.001f);
    }

    [TestMethod]
    public void BothHeightSitesUseTheBudget() {
        StringAssert.Contains(Dock("DockAccordion.cs"), "DockBodyBudget.For");
        StringAssert.Contains(Dock("InvestitureDockWindow.cs"), "DockBodyBudget.For");
    }
}
