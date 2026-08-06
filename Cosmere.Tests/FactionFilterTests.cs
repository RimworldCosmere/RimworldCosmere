using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Cosmere scenarios filter the faction list down to Cosmere factions. Hidden factions have
///     to survive that: vanilla treats them as always-present infrastructure, and a missing
///     Ancients makes Faction.OfAncients null, which NREs PawnGenerator mid-generation.
/// </summary>
[TestClass]
public class FactionFilterTests {
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

    private static string PatchSource {
        get {
            string path = Path.Combine(
                RepoRoot, "CosmereCore", "CosmereCore", "Core", "Patch", "World", "FactionGeneratorPatches.cs"
            );

            Assert.IsTrue(File.Exists(path), $"Expected the faction filter at {path}");
            return File.ReadAllText(path);
        }
    }

    [TestMethod]
    public void HiddenFactionsBypassTheScenarioFilter() {
        Assert.IsTrue(
            PatchSource.Contains("faction.hidden"),
            "Without a hidden-faction bypass, Ancients is never created and Faction.OfAncients is null. " +
            "That NREs PawnGenerator during relation generation, which strands starting pawns without gear."
        );
    }

    [TestMethod]
    public void HiddenBypassRunsAfterTheSettingsToggles() {
        string source = PatchSource;

        int empireToggle = source.IndexOf("disableEmpireInCosmereScenarios", StringComparison.Ordinal);
        int odysseyToggle = source.IndexOf("disableOdysseyFactionsInCosmereScenarios", StringComparison.Ordinal);
        int hiddenBypass = source.IndexOf("faction.hidden", StringComparison.Ordinal);

        Assert.IsTrue(empireToggle >= 0 && odysseyToggle >= 0, "Expected both faction settings toggles.");
        Assert.IsTrue(
            hiddenBypass > empireToggle && hiddenBypass > odysseyToggle,
            "The hidden bypass has to come after the settings toggles, or it overrides the player's choice."
        );
    }

    /// <summary>
    ///     The Odyssey toggle spent its whole life comparing against "MechanoidHive" and
    ///     "InsectGeneline". Those are the labels. The defNames are "Mechanoid" and "Insect", so
    ///     the setting matched nothing and both factions turned up in every Cosmere scenario.
    /// </summary>
    [TestMethod]
    public void SettingsMatchFactionDefNamesNotLabels() {
        string source = PatchSource;

        foreach (string label in new[] { "MechanoidHive", "InsectGeneline", "ShatteredEmpire" }) {
            Assert.IsFalse(
                source.Contains($"\"{label}\"", StringComparison.Ordinal),
                $"\"{label}\" is a faction label, not a defName - the comparison can never match."
            );
        }

        foreach (string defName in new[] { "Mechanoid", "Insect", "Empire" }) {
            Assert.IsTrue(
                source.Contains($"\"{defName}\"", StringComparison.Ordinal),
                $"Expected the filter to name the {defName} faction by its defName."
            );
        }
    }

    /// <summary>
    ///     Vanilla shouts in yellow when Empire, Mechanoid or Insect are missing from the worldgen
    ///     faction list. Those three get concealed rather than dropped so the page stays quiet,
    ///     and the create-faction gate is what actually keeps them out of the world.
    /// </summary>
    [TestMethod]
    public void WarnedAboutFactionsAreConcealedNotDropped() {
        string source = PatchSource;

        Assert.IsTrue(
            source.Contains("IsWarnedAboutWhenMissing", StringComparison.Ordinal),
            "Expected the filter to know which factions vanilla warns about losing."
        );

        Assert.IsTrue(
            source.Contains("displayInFactionSelection = false", StringComparison.Ordinal),
            "Concealing means clearing displayInFactionSelection, so the row and the Add entry go away."
        );

        Assert.IsTrue(
            source.Contains("displayInFactionSelection = true", StringComparison.Ordinal),
            "A concealed faction has to be revealed again, or a later vanilla scenario keeps the hidden row."
        );

        int concealCheck = source.IndexOf("IsWarnedAboutWhenMissing(faction)", StringComparison.Ordinal);
        int yieldAfter = source.IndexOf("yield return faction;", concealCheck, StringComparison.Ordinal);
        Assert.IsTrue(
            concealCheck >= 0 && yieldAfter >= 0,
            "A concealed faction still has to be yielded, or it leaves the list and vanilla warns anyway."
        );
    }
}
