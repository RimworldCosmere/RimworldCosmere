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
}
