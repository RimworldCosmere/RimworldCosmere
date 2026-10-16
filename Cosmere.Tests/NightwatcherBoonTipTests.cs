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

    private static string Core(params string[] parts) {
        return File.ReadAllText(Path.Combine([RepoRoot, "CosmereCore", "CosmereCore", .. parts]));
    }

    [TestMethod]
    public void DialogNoLongerOwnsTheEffectText() {
        string dialog = Core("System", "Roshar", "Dialog", "Dialog_NightwatcherEncounter.cs");

        Assert.Contains(
            "NightwatcherEffectText.Boon(",
            dialog,
            "The dialog must read its effect text from the shared class."
        );
        Assert.DoesNotContain(
            "static string GetBoonEffects(",
            dialog,
            "GetBoonEffects moved to NightwatcherEffectText; a private copy would drift from the tooltip."
        );
        Assert.DoesNotContain(
            "static string GetCurseEffects(",
            dialog,
            "GetCurseEffects moved to NightwatcherEffectText; a private copy would drift from the tooltip."
        );
    }

    [TestMethod]
    public void BoonHediffTooltipShowsTheEffects() {
        string hediff = Core("System", "Roshar", "Hediff", "NightwatcherBoonHediff.cs");

        Assert.Contains("override string TipStringExtra", hediff, "The boon hediff needs its own tooltip.");
        Assert.Contains("NightwatcherEffectText", hediff, "The tooltip must use the shared effect text.");
    }

    [TestMethod]
    public void BoonHediffRemembersTheChosenMetal() {
        string hediff = Core("System", "Roshar", "Hediff", "NightwatcherBoonHediff.cs");
        string applicator = Core("System", "Roshar", "Nightwatcher", "StandardBoonApplicator.cs");

        Assert.Contains(
            "NightwatcherEffectText.Boon(boonDef, choiceKey",
            hediff,
            "Without the choice key, Misting and Ferring show 'choose metal' forever."
        );
        Assert.Contains(
            "Scribe_Values.Look(ref choiceKey",
            hediff,
            "The choice has to survive a save/load or the tooltip reverts after a reload."
        );
        Assert.Contains(
            "Initialize(boon, context?.SelectedDefName)",
            applicator,
            "The applicator is the only place that knows what the pawn picked."
        );
    }

    [TestMethod]
    public void CurseHediffTooltipShowsTheEffects() {
        string hediff = Core("System", "Roshar", "Hediff", "NightwatcherCurseHediff.cs");

        Assert.Contains("override string TipStringExtra", hediff, "The curse hediff needs its own tooltip.");
        Assert.Contains("NightwatcherEffectText.Curse(", hediff, "The tooltip must use the shared effect text.");
    }
}
