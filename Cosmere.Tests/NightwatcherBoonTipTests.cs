using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     A boon with no hediff of its own used to land as a bare label. The picker and the tooltip
///     read the same effect text now, so neither can drift from the other.
/// </summary>
[TestClass]
public class NightwatcherBoonTipTests {
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

    private static string Core(params string[] parts) => File.ReadAllText(Path.Combine(
        [RepoRoot, "CosmereCore", "CosmereCore", .. parts]
    ));

    [TestMethod]
    public void DialogNoLongerOwnsTheEffectText() {
        string dialog = Core("System", "Roshar", "Dialog", "Dialog_NightwatcherEncounter.cs");

        StringAssert.Contains(
            dialog,
            "NightwatcherEffectText.Boon(",
            "The dialog must read its effect text from the shared class."
        );
        Assert.IsFalse(
            dialog.Contains("static string GetBoonEffects("),
            "GetBoonEffects moved to NightwatcherEffectText; a private copy would drift from the tooltip."
        );
        Assert.IsFalse(
            dialog.Contains("static string GetCurseEffects("),
            "GetCurseEffects moved to NightwatcherEffectText; a private copy would drift from the tooltip."
        );
    }

    [TestMethod]
    public void BoonHediffTooltipShowsTheEffects() {
        string hediff = Core("System", "Roshar", "Hediff", "NightwatcherBoonHediff.cs");

        StringAssert.Contains(hediff, "override string TipStringExtra", "The boon hediff needs its own tooltip.");
        StringAssert.Contains(hediff, "NightwatcherEffectText", "The tooltip must use the shared effect text.");
    }

    [TestMethod]
    public void BoonHediffRemembersTheChosenMetal() {
        string hediff = Core("System", "Roshar", "Hediff", "NightwatcherBoonHediff.cs");
        string applicator = Core("System", "Roshar", "Nightwatcher", "StandardBoonApplicator.cs");

        StringAssert.Contains(
            hediff,
            "NightwatcherEffectText.Boon(boonDef, choiceKey",
            "Without the choice key, Misting and Ferring show 'choose metal' forever."
        );
        StringAssert.Contains(
            hediff,
            "Scribe_Values.Look(ref choiceKey",
            "The choice has to survive a save/load or the tooltip reverts after a reload."
        );
        StringAssert.Contains(
            applicator,
            "Initialize(boon, context?.SelectedDefName)",
            "The applicator is the only place that knows what the pawn picked."
        );
    }

    [TestMethod]
    public void CurseHediffTooltipShowsTheEffects() {
        string hediff = Core("System", "Roshar", "Hediff", "NightwatcherCurseHediff.cs");

        StringAssert.Contains(hediff, "override string TipStringExtra", "The curse hediff needs its own tooltip.");
        StringAssert.Contains(hediff, "NightwatcherEffectText.Curse(", "The tooltip must use the shared effect text.");
    }
}
