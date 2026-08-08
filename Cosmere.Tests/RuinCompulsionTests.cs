using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards how Ruin uses a heavily spiked pawn: it announces itself, and it can be stopped.
/// </summary>
[TestClass]
public class RuinCompulsionTests {
    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir);
            return dir!.FullName;
        }
    }

    private static string Source(string file) => File.ReadAllText(Path.Combine(
        RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Hemalurgy", file
    ));

    /// <summary>
    ///     The warning is the whole design. Firing the mental state the moment Ruin decides would
    ///     be the old slot machine with extra steps.
    /// </summary>
    [TestMethod]
    public void RuinWarnsBeforeItActs() {
        string hediff = Source(Path.Combine("Hediff", "RuinsInfluence.cs"));

        Assert.IsTrue(hediff.Contains("RuinCompulsions.Warn(pawn, compulsion)", StringComparison.Ordinal));
        Assert.IsTrue(hediff.Contains("pendingTick", StringComparison.Ordinal), "The act has to be scheduled.");
        Assert.IsTrue(
            hediff.Contains("Find.TickManager.TicksGame < pendingTick", StringComparison.Ordinal),
            "It must wait for the window to close before acting."
        );
    }

    /// <summary>
    ///     Downing, arresting or sedating the pawn is the intended answer, so the check has to run
    ///     again when the compulsion fires rather than only when it was decided.
    /// </summary>
    [TestMethod]
    public void StoppingThePawnStopsTheCompulsion() {
        string hediff = Source(Path.Combine("Hediff", "RuinsInfluence.cs"));
        int act = hediff.IndexOf("private void Act()", StringComparison.Ordinal);
        Assert.IsTrue(act >= 0);

        Assert.IsTrue(
            hediff[act..].Contains("RuinCompulsions.CanBeMoved(pawn)", StringComparison.Ordinal),
            "Act must recheck that the pawn can still be used."
        );

        string compulsions = Source("RuinCompulsions.cs");
        foreach (string guard in new[] { "pawn.Downed", "pawn.InMentalState", "pawn.IsPrisoner", "pawn.Dead" }) {
            Assert.IsTrue(compulsions.Contains(guard, StringComparison.Ordinal), $"CanBeMoved should consider {guard}.");
        }
    }

    /// <summary>Pulling a spike in time has to call off whatever was coming.</summary>
    [TestMethod]
    public void FallingBelowTheThresholdCancelsIt() {
        string hediff = Source(Path.Combine("Hediff", "RuinsInfluence.cs"));
        int update = hediff.IndexOf("public void UpdateSpikeCount(", StringComparison.Ordinal);
        Assert.IsTrue(update >= 0);

        Assert.IsTrue(
            hediff[update..(update + 400)].Contains("Clear()", StringComparison.Ordinal),
            "Dropping below the control threshold should clear the pending compulsion."
        );
    }

    /// <summary>Every compulsion needs both letter keys, or the warning arrives blank.</summary>
    [TestMethod]
    public void EveryCompulsionHasItsLetter() {
        string compulsions = Source("RuinCompulsions.cs");
        string keyed = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereScadrial", "Languages", "English", "Keyed", "Messages.xml"
        ));

        foreach (string key in new[] { "Wander", "Violence", "Fire", "Slaughter" }) {
            Assert.IsTrue(compulsions.Contains($"key = \"{key}\"", StringComparison.Ordinal), $"{key} is not declared.");
            Assert.IsTrue(keyed.Contains($"CS_Ruin_Compulsion_{key}_Title", StringComparison.Ordinal), $"{key} has no title.");
            Assert.IsTrue(keyed.Contains($"CS_Ruin_Compulsion_{key}_Text", StringComparison.Ordinal), $"{key} has no body.");
        }
    }
}
