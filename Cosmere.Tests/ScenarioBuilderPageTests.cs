using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards that every Cosmere scenario shows the character builder, and shows it in an order
///     that survives our named story pawns.
/// </summary>
[TestClass]
public class ScenarioBuilderPageTests {
    private const string NamedPawns = "Cosmere.Core.ScenarioPart.Parts.ScenPart_NamedPawns";

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

    private static IEnumerable<(string name, List<XElement> parts)> Scenarios() {
        foreach (string mod in new[] { "CosmereCore", "CosmereScadrial", "CosmereRoshar" }) {
            string defs = Path.Combine(RepoRoot, mod, "Defs");
            if (!Directory.Exists(defs)) continue;

            foreach (string file in Directory.GetFiles(defs, "*.xml", SearchOption.AllDirectories)) {
                XDocument doc;
                try {
                    doc = XDocument.Load(file);
                } catch (Exception) {
                    continue;
                }

                if (doc.Root == null) continue;
                foreach (XElement scenario in doc.Root.Elements("ScenarioDef")) {
                    string? name = scenario.Element("defName")?.Value;
                    if (name == null || !name.StartsWith("Cosmere_", StringComparison.Ordinal)) continue;

                    List<XElement> parts = scenario.Element("scenario")?.Element("parts")?
                        .Elements("li").ToList() ?? [];
                    yield return (name, parts);
                }
            }
        }
    }

    private static string ClassOf(XElement part) {
        return (string?)part.Attribute("Class") ?? string.Empty;
    }

    [TestMethod]
    public void EveryScenarioShowsTheCharacterBuilder() {
        foreach ((string name, List<XElement> parts) in Scenarios()) {
            Assert.IsTrue(
                parts.Any(p => ClassOf(p).Contains("ConfigPage", StringComparison.Ordinal)),
                $"{name} has no ConfigPage part, so it skips the character builder."
            );
        }
    }

    /// <summary>
    ///     The config page clears the roster in GenerateStartingPawns and regenerates it, and
    ///     ScenPart_NamedPawns stamps its templates onto each pawn through Notify_PawnGenerated
    ///     as it comes out. So the config page has to be the last word, not the first.
    /// </summary>
    [TestMethod]
    public void TheBuilderRunsAfterTheNamedPawns() {
        foreach ((string name, List<XElement> parts) in Scenarios()) {
            int config = parts.FindIndex(p => ClassOf(p).Contains("ConfigPage", StringComparison.Ordinal));
            int named = parts.FindIndex(p => ClassOf(p) == NamedPawns);
            if (named < 0) continue;

            Assert.IsTrue(config > named, $"{name} runs the config page before NamedPawns.");
        }
    }

    /// <summary>
    ///     ScenPart_ConfigPage_ConfigureStartingPawns_Xenotypes.PostIdeoChosen calls
    ///     xenotypeCounts.Where without a null check, and the field has no default. Leaving the
    ///     element out throws on every start of that scenario.
    /// </summary>
    [TestMethod]
    public void EveryXenotypeConfigPageDeclaresXenotypeCounts() {
        foreach ((string name, List<XElement> parts) in Scenarios()) {
            foreach (XElement part in parts) {
                if (!ClassOf(part).EndsWith("ConfigureStartingPawns_Xenotypes", StringComparison.Ordinal)) continue;

                Assert.IsNotNull(
                    part.Element("xenotypeCounts"),
                    $"{name} omits xenotypeCounts, which NREs in PostIdeoChosen."
                );
            }
        }
    }
}
