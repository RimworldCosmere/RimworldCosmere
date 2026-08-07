using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
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
    ///     Nobody drops out of the sky on Scadrial or Roshar.
    /// </summary>
    /// <remarks>
    ///     Spaceflight is Scadrial's fourth era, centuries past anything these scenarios cover,
    ///     and Roshar never reaches it. A raid arriving by pod reads as a different game.
    /// </remarks>
    [TestMethod]
    public void NoCosmereFactionArrivesByDropPod() {
        string[] bases = [
            Path.Combine("CosmereScadrial", "Defs", "Factions", "NPCFactions.xml"),
            Path.Combine("CosmereScadrial", "Defs", "Factions", "ScenarioFactions.xml"),
            Path.Combine("CosmereRoshar", "Defs", "Factions", "NPCFactions.xml"),
            Path.Combine("CosmereRoshar", "Defs", "Factions", "ScenarioFactions.xml"),
        ];

        List<string> offenders = [];
        foreach (string relative in bases) {
            string path = Path.Combine(RepoRoot, relative);
            if (!File.Exists(path)) {
                offenders.Add($"{relative} is missing");
                continue;
            }

            string source = File.ReadAllText(path);
            if (!source.Contains("arrivalModeBlacklist", StringComparison.Ordinal)) {
                offenders.Add($"{relative} lets its factions arrive by pod");
                continue;
            }

            foreach (string mode in new[] { "CenterDrop", "EdgeDrop", "EdgeDropGroups", "RandomDrop" }) {
                if (!source.Contains($"<li>{mode}</li>", StringComparison.Ordinal)) {
                    offenders.Add($"{relative} does not block {mode}");
                }
            }
        }

        Assert.AreEqual(0, offenders.Count, string.Join("; ", offenders));
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
        int yieldAfter = concealCheck < 0
            ? -1
            : source.IndexOf("yield return faction;", concealCheck, StringComparison.Ordinal);

        Assert.IsTrue(
            concealCheck >= 0 && yieldAfter >= 0,
            "A concealed faction still has to be yielded, or it leaves the list and vanilla warns anyway."
        );
    }

    /// <summary>
    ///     The predicate used to ask whether the scenario name started with "Stormlight:",
    ///     "Roshar:" or "Cosmere:". ScenarioDef.PostLoad copies label into scenario.name, and
    ///     every shipped label reads "Cosmere - Scadrial - The Final Empire", so none of the three
    ///     ever matched. All seven Roshar scenarios got no filtering at all, and only Scadrial
    ///     worked - by the accident of a Contains("Scadrial") sitting next to them.
    /// </summary>
    [TestMethod]
    public void ScenarioRecognitionDoesNotGuessAtLabels() {
        string source = PatchSource;

        foreach (string prefix in new[] { "Stormlight:", "Roshar:", "Cosmere:", "Mistborn:" }) {
            Assert.IsFalse(
                source.Contains(prefix, StringComparison.Ordinal),
                $"\"{prefix}\" is not how any shipped scenario is labelled - the match can never fire."
            );
        }

        Assert.IsTrue(
            source.Contains("ScenarioDefUtility.IsCosmere", StringComparison.Ordinal),
            "Whether a scenario is ours comes from the mod that declares it, not from its label."
        );
    }

    /// <summary>
    ///     Every Cosmere faction is named Cosmere_&lt;World&gt;_Faction_*, and WorldForFaction reads
    ///     the world out of that. One badly named faction resolves to no world and disappears from
    ///     its own world's scenarios, with nothing logged.
    /// </summary>
    [TestMethod]
    public void EveryCosmereFactionNamesItsWorld() {
        List<string> worlds = [];
        List<string> factions = [];

        foreach (string mod in new[] { "CosmereCore", "CosmereScadrial", "CosmereRoshar" }) {
            string dir = Path.Combine(RepoRoot, mod, "Defs");
            if (!Directory.Exists(dir)) continue;

            foreach (string path in Directory.GetFiles(dir, "*.xml", SearchOption.AllDirectories)) {
                XElement? root = XDocument.Load(path).Root;
                if (root == null) continue;

                foreach (XElement world in root.Descendants("Cosmere.Core.Def.CosmereWorldDef")) {
                    // The cross-world sentinel is nobody's home, so it is not a token.
                    if (world.Element("crossWorld")?.Value.Trim() == "true") continue;

                    string? name = world.Element("defName")?.Value.Trim();
                    if (!string.IsNullOrEmpty(name)) worlds.Add(name!);
                }

                foreach (XElement faction in root.Descendants("FactionDef")) {
                    string? name = faction.Element("defName")?.Value.Trim();
                    if (!string.IsNullOrEmpty(name)) factions.Add(name!);
                }
            }
        }

        Assert.IsTrue(worlds.Count > 0, "Found no world defs - the walk is wrong, not the data.");
        Assert.IsTrue(factions.Count > 0, "Found no faction defs - the walk is wrong, not the data.");

        List<string> offenders = [];
        foreach (string faction in factions) {
            int matches = 0;
            foreach (string world in worlds) {
                if (faction.Contains(world, StringComparison.Ordinal)) matches++;
            }

            if (matches != 1) offenders.Add($"{faction} matches {matches} worlds");
        }

        Assert.AreEqual(
            0,
            offenders.Count,
            "Each faction has to name exactly one world: " + string.Join("; ", offenders)
        );
    }

    /// <summary>
    ///     CreateFactionAndAddToManager is not only worldgen's funnel - the single-argument
    ///     overload forwards to it, and ScenPart_FactionRelations, CreateFactionAction and
    ///     RaidAction all call that. An unconditional cancel makes a scripted story faction fail
    ///     to arrive with nothing logged.
    ///     <para>
    ///         The exemption is opt-in from our own callers rather than a flag set around world
    ///         generation. If a worldgen flag ever failed to attach, the gate would silently stop
    ///         filtering anything - the more expensive way to be wrong.
    ///     </para>
    /// </summary>
    [TestMethod]
    public void OnlyScriptedCreationBypassesTheWorldGate() {
        string source = PatchSource;

        Assert.IsTrue(
            source.Contains("IsScriptedCreation", StringComparison.Ordinal),
            "Expected scripted creation to be distinguishable from world generation."
        );

        int guard = source.IndexOf(
            "if (FactionGeneratorPatch.IsScriptedCreation) return Control.Continue;",
            StringComparison.Ordinal
        );
        int cancel = source.IndexOf("Control.Cancel", StringComparison.Ordinal);
        Assert.IsTrue(
            guard >= 0 && cancel > guard,
            "The scripted check has to run before the cancel, or story beats are still swallowed."
        );
    }

    /// <summary>
    ///     Every scripted beat has to go through CreateScripted. A direct FactionGenerator call
    ///     lands on the gate and the faction never arrives.
    /// </summary>
    [TestMethod]
    public void NoScriptedBeatCallsFactionGeneratorDirectly() {
        string coreDir = Path.Combine(RepoRoot, "CosmereCore", "CosmereCore");
        string gate = Path.Combine("Core", "Patch", "World", "FactionGeneratorPatches.cs");

        List<string> offenders = [];
        foreach (string path in Directory.GetFiles(coreDir, "*.cs", SearchOption.AllDirectories)) {
            string relative = Path.GetRelativePath(coreDir, path);

            // The gate itself is the one legitimate caller - CreateScripted wraps it.
            if (relative == gate) continue;

            if (File.ReadAllText(path)
                .Contains("FactionGenerator.CreateFactionAndAddToManager", StringComparison.Ordinal)) {
                offenders.Add(relative);
            }
        }

        Assert.AreEqual(
            0,
            offenders.Count,
            "These call FactionGenerator directly and will be filtered out: " + string.Join(", ", offenders)
        );
    }
}
