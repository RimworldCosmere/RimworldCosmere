using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     ScenPart_FactionRelations only ever set player-to-faction goodwill, so every faction a
///     scenario created started neutral to every other one. The skaa and the empire sat out the
///     Collapse being polite to each other.
/// </summary>
[TestClass]
public class FactionHostilityTests {
    private const string Skaa = "Cosmere_Scadrial_Faction_SkaaRebels";
    private const string Empire = "Cosmere_Scadrial_Faction_FinalEmpireNPC";

    /// <summary>Eras where the rebellion and the empire are actively at war.</summary>
    private static readonly string[] CollapseEraScenarios = [
        "FinalEmpire.xml", "WellOfAscension.xml", "HeroOfAges.xml", "PreCatacendre.xml",
    ];

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

    private static string ScenarioDirectory => Path.Combine(RepoRoot, "CosmereScadrial", "Defs", "Scenarios");

    [TestMethod]
    public void CollapseEraScenariosPutTheSkaaAtWarWithTheEmpire() {
        if (!Directory.Exists(ScenarioDirectory)) return;

        List<string> offenders = [];

        foreach (string file in CollapseEraScenarios) {
            string path = Path.Combine(ScenarioDirectory, file);
            if (!File.Exists(path)) {
                offenders.Add($"{file} is missing");
                continue;
            }

            XElement? root = XDocument.Load(path).Root;
            if (root == null) continue;

            bool hostile = false;
            foreach (XElement pair in root.Descendants("between").Elements("li")) {
                string? a = pair.Element("a")?.Value;
                string? b = pair.Element("b")?.Value;
                string? goodwill = pair.Element("goodwill")?.Value;
                if (a == null || b == null || goodwill == null) continue;

                bool names = (a == Skaa && b == Empire) || (a == Empire && b == Skaa);
                if (names && int.TryParse(goodwill, out int value) && value <= -75) hostile = true;
            }

            if (!hostile) offenders.Add(file);
        }

        string detail = "These collapse-era scenarios do not put the Skaa Rebellion at war with the " +
            "Final Empire, so the two sit out the Collapse neutral to each other: " +
            string.Join(", ", offenders);

        Assert.AreEqual(0, offenders.Count, detail);
    }

    /// <summary>A pair naming a faction twice, or a def we do not ship, quietly does nothing.</summary>
    [TestMethod]
    public void EveryHostilityPairIsWellFormed() {
        if (!Directory.Exists(ScenarioDirectory)) return;

        HashSet<string> factions = [];
        foreach (string mod in new[] { "CosmereCore", "CosmereScadrial", "CosmereRoshar" }) {
            string dir = Path.Combine(RepoRoot, mod, "Defs");
            if (!Directory.Exists(dir)) continue;

            foreach (string path in Directory.GetFiles(dir, "*.xml", SearchOption.AllDirectories)) {
                XElement? root = XDocument.Load(path).Root;
                if (root == null) continue;
                foreach (XElement def in root.Descendants("FactionDef")) {
                    string? name = def.Element("defName")?.Value;
                    if (name != null) factions.Add(name);
                }
            }
        }

        List<string> offenders = [];
        foreach (string path in Directory.GetFiles(ScenarioDirectory, "*.xml")) {
            XElement? root = XDocument.Load(path).Root;
            if (root == null) continue;

            string file = Path.GetFileName(path);
            foreach (XElement pair in root.Descendants("between").Elements("li")) {
                string? a = pair.Element("a")?.Value;
                string? b = pair.Element("b")?.Value;

                if (a == null || b == null) {
                    offenders.Add($"{file}: a pair is missing <a> or <b>");
                    continue;
                }

                if (a == b) offenders.Add($"{file}: '{a}' paired with itself");
                if (!factions.Contains(a)) offenders.Add($"{file}: '{a}' is not a FactionDef we ship");
                if (!factions.Contains(b)) offenders.Add($"{file}: '{b}' is not a FactionDef we ship");
            }
        }

        Assert.AreEqual(0, offenders.Count, string.Join("; ", offenders));
    }
}
