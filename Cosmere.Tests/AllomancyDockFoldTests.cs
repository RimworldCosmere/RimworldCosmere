using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The allomancy dock pins what is burning and names every ability group.
/// </summary>
/// <remarks>
///     No Unity or Verse loads here, so this guards the source instead.
/// </remarks>
[TestClass]
public class AllomancyDockFoldTests {
    private static string UISource(string file) {
        DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
            dir = dir.Parent;
        }

        Assert.IsNotNull(dir, "Could not locate CosmereScadrial/Defs above the test output directory.");

        return File.ReadAllText(Path.Combine(dir.FullName, "CosmereCore", "CosmereCore", "System", "Scadrial", "UI", file));
    }

    [TestMethod]
    public void BurningMetalsArePinnedAboveTheScrollView() {
        string source = UISource("AllomancyDockSection.cs");
        Assert.Contains("override void DrawPinned", source, "A folded or scrolled table must not hide a draining metal.");
        Assert.Contains("CC_Dock_Burning_Header", source, "The pinned strip needs its own heading.");
    }

    [TestMethod]
    public void EveryAbilityGroupGetsItsHeading() {
        string source = UISource("AbilityRowLayout.cs");
        Assert.IsFalse(
            source.Contains("ShowGroupHeaders", StringComparison.Ordinal),
            "A metal with one kind of ability still needs the heading that says which kind it is."
        );
    }

    [TestMethod]
    public void BurningRowLabelMatchesThePinnedStrip() {
        string source = UISource("AllomancyDockSection.cs");
        Assert.Contains(
            "ability.IsFlaring ? FlaringTint : DockPalette.HotLabel",
            source,
            "A burning row is the one the player must read first, so it uses the same hot label as the pinned strip."
        );
    }
}
