using System;
using System.Collections.Generic;
using System.IO;
using Cosmere.System.Scadrial.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Dock chrome carries state in weight and spacing, not in washes of colour, and the strip a
///     section pins above its body has to sit outside the scroll view to stay put.
/// </summary>
[TestClass]
public class DockChromeTests {
    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereScadrial/Defs above the test output directory.");

            return dir.FullName;
        }
    }

    private static string Dock(string file) => File.ReadAllText(Path.Combine(
        RepoRoot, "CosmereCore", "CosmereCore", "Core", "UI", "Dock", file
    ));

    /// <summary>
    ///     The same file with every comment gone, so a comment explaining the fill we removed does
    ///     not read as the fill itself.
    /// </summary>
    private static string CodeOnly(string file) {
        List<string> kept = [];

        foreach (string line in Dock(file).Split('\n')) {
            string trimmed = line.TrimStart();
            if (trimmed.StartsWith("//") || trimmed.StartsWith("*")) continue;

            kept.Add(line);
        }

        return string.Join("\n", kept);
    }

    /// <summary>
    ///     The plumbing is worthless without a section that uses it. Allomancy is the one that has
    ///     to pin: a burning metal drains whether or not the table is scrolled to it.
    /// </summary>
    [TestMethod]
    public void AllomancyOverridesThePinnedStrip() {
        string section = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "UI", "AllomancyDockSection.cs"
        ));

        StringAssert.Contains(section, "override float GetPinnedHeight", "Allomancy has to size its pinned strip.");
        StringAssert.Contains(section, "override void DrawPinned", "Allomancy has to draw its pinned strip.");
        StringAssert.Contains(section, "BurnReservePercentPerSecond(pawn, cell, ability)", "The strip reports the burn rate.");
    }

    /// <summary>
    ///     The pinned strip belongs above the scroll view. Drawn inside it, the one row that must
    ///     stay visible scrolls away with everything else. Asserting the whole wiring, not just the
    ///     call, so deleting any part of it fails here instead of going quietly dead.
    /// </summary>
    [TestMethod]
    public void ThePinnedStripDrawsOutsideTheScrollView() {
        string accordion = CodeOnly("DockAccordion.cs");

        int height = accordion.IndexOf("section.GetPinnedHeight(", StringComparison.Ordinal);
        int pinned = accordion.IndexOf("section.DrawPinned(", StringComparison.Ordinal);
        int scroll = accordion.IndexOf("BeginScrollView", StringComparison.Ordinal);

        Assert.IsTrue(height >= 0, "The accordion has to ask the section how tall its pinned strip is.");
        Assert.IsTrue(pinned >= 0, "The accordion has to give a section its pinned slot.");
        Assert.IsTrue(scroll >= 0, "A long body still scrolls.");
        Assert.IsTrue(height < pinned, "The height query comes before the draw.");
        Assert.IsTrue(pinned < scroll, "The pinned strip draws before the scroll view opens.");

        StringAssert.Contains(accordion, "pinnedHeight > 0f", "A section with no strip gets no slot.");
        StringAssert.Contains(accordion, "y += pinnedHeight", "The body starts below the strip, not under it.");
    }

    /// <summary>
    ///     The window has to count the pinned strip too. Left out, the dock sizes short by exactly
    ///     the strip's height, so burning a metal pushed the grid into a scroll view.
    /// </summary>
    [TestMethod]
    public void TheWindowSizesItselfAroundThePinnedStrip() {
        string window = CodeOnly("InvestitureDockWindow.cs");

        StringAssert.Contains(
            window, "GetPinnedHeight(", "The height total has to include the pinned strip."
        );
        StringAssert.Contains(
            window, "DockBodyBudget.For", "The body cap drops by the strip, the same way the accordion caps it."
        );
    }

    /// <summary>
    ///     Nine metals burning is a normal fight for a Mistborn, and every one of them has to show.
    ///     A capped strip hid the rest behind a count, so a folded quadrant made them invisible.
    /// </summary>
    [TestMethod]
    public void ThePinnedStripListsEveryBurningMetal() {
        string section = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "UI", "AllomancyDockSection.cs"
        ));

        Assert.IsFalse(
            section.Contains("MaxBurningRows", StringComparison.Ordinal),
            "A row cap hides a draining metal, which is the one thing the strip exists to prevent."
        );

        StringAssert.Contains(
            section,
            "BurningStripLayout.HeightFor(BurningRowCount(",
            "The pinned height has to follow the real burning count."
        );
    }

    /// <summary>
    ///     A strip taller than the dock leaves the body nothing, and the budget has to clamp to
    ///     zero instead of handing the scroll view a negative rect.
    /// </summary>
    [TestMethod]
    public void AnOversizedStripClampsTheBodyToZero() {
        Assert.AreEqual(0f, Cosmere.Core.UI.Dock.DockBodyBudget.For(400f, 150f, BurningStripLayout.HeightFor(9)), 0.001f);
    }
}
