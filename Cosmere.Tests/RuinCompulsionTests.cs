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
            return dir.FullName;
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

    /// <summary>
    ///     After the Catacendre, Harmony holds both Shards. A spiked colonist in the Alloy era
    ///     hearing Ruin by name reads as a bug rather than as history, so nothing player-facing
    ///     may hardcode the name.
    /// </summary>
    [TestMethod]
    public void TheVoiceIsNamedForTheLiveShard() {
        string resolver = Source("HemalurgicShard.cs");
        Assert.IsTrue(resolver.Contains("ShardDefOf.Harmony", StringComparison.Ordinal));
        Assert.IsTrue(resolver.Contains("ShardDefOf.Ruin", StringComparison.Ordinal));

        string thought = Source("Thought_ShardInfluence.cs");
        Assert.IsTrue(thought.Contains("HemalurgicShard.Name", StringComparison.Ordinal));

        string compulsions = Source("RuinCompulsions.cs");
        Assert.IsTrue(
            compulsions.Contains("HemalurgicShard.Name.Named(\"SHARD\")", StringComparison.Ordinal),
            "The warning letters should name the live Shard."
        );

        // braces are the grammar resolver's syntax, run before substitution - using them in a label logs unresolvable.
        string thoughtDefs = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereScadrial", "Defs", "Hemalurgy", "Thoughts.xml"
        ));
        Assert.IsFalse(
            thoughtDefs.Contains("{SHARD}", StringComparison.Ordinal),
            "Thought text must use [SHARD], which the grammar resolver ignores."
        );
    }

    /// <summary>
    ///     The stage labels and letters have to carry the placeholder, or the substitution has
    ///     nothing to replace and the text silently keeps saying Ruin.
    /// </summary>
    [TestMethod]
    public void TheInfluenceTextUsesThePlaceholder() {
        string thoughts = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereScadrial", "Defs", "Hemalurgy", "Thoughts.xml"
        ));

        Assert.IsTrue(
            thoughts.Contains("Cosmere.System.Scadrial.Hemalurgy.Thought_ShardInfluence", StringComparison.Ordinal),
            "The thought needs the class that does the substitution."
        );

        XDocument doc = XDocument.Parse(thoughts);
        XElement whispers = doc.Descendants("ThoughtDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Thought_RuinsWhispers");

        foreach (XElement stage in whispers.Element("stages")!.Elements("li")) {
            string label = stage.Element("label")?.Value ?? string.Empty;
            Assert.IsTrue(label.Contains("[SHARD]", StringComparison.Ordinal), $"'{label}' hardcodes a Shard name.");
        }
    }
}
