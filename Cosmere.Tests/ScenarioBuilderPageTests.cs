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
    ///     xenotypeCounts has to exist and has to add up to something.
    /// </summary>
    /// <remarks>
    ///     PostIdeoChosen calls <c>xenotypeCounts.Where</c> with no null check, so omitting the
    ///     element throws outright. Worse, an empty list parses fine and makes TotalPawnCount
    ///     zero, which sets startingPawnCount to zero and blows up in DoDropPods during map
    ///     generation, a long way from the cause. A sum above pawnChoiceCount is fine: that just
    ///     means extra optional picks.
    /// </remarks>
    [TestMethod]
    public void EveryXenotypeConfigPageStartsWithAtLeastOnePawn() {
        foreach ((string name, List<XElement> parts) in Scenarios()) {
            foreach (XElement part in parts) {
                if (!ClassOf(part).EndsWith("ConfigureStartingPawns_Xenotypes", StringComparison.Ordinal)) continue;

                XElement? counts = part.Element("xenotypeCounts");
                Assert.IsNotNull(counts, $"{name} omits xenotypeCounts, which NREs in PostIdeoChosen.");

                int total = counts!.Elements("li")
                    .Sum(li => int.TryParse(li.Element("count")?.Value, out int c) ? c : 0);

                Assert.IsTrue(
                    total > 0,
                    $"{name} has xenotypeCounts summing to {total}. That starts the colony with no pawns."
                );
            }
        }
    }

    /// <summary>
    ///     The builder count has to keep up with the roster.
    /// </summary>
    /// <remarks>
    ///     Adding a named pawn without raising pawnChoiceCount and the xenotypeCounts total
    ///     leaves the colony short a person, and getting that total wrong is what broke map
    ///     generation once already. Scenarios with no named pawns are free to offer more choices
    ///     than they require, which is ordinary vanilla behaviour.
    /// </remarks>
    [TestMethod]
    public void NamedPawnScenariosSizeTheBuilderToTheirRoster() {
        foreach ((string name, List<XElement> parts) in Scenarios()) {
            XElement? named = parts.FirstOrDefault(p => ClassOf(p) == NamedPawns);
            if (named == null) continue;

            int roster = named.Element("pawns")?.Elements("li").Count() ?? 0;
            if (roster == 0) continue;

            XElement? config = parts.FirstOrDefault(
                p => ClassOf(p).EndsWith("ConfigureStartingPawns_Xenotypes", StringComparison.Ordinal)
            );
            Assert.IsNotNull(config, $"{name} has named pawns but no config page.");

            Assert.AreEqual(
                roster,
                int.Parse(config!.Element("pawnChoiceCount")!.Value),
                $"{name}: pawnChoiceCount does not match its {roster} named pawns."
            );

            int total = config.Element("xenotypeCounts")!.Elements("li")
                .Sum(li => int.TryParse(li.Element("count")?.Value, out int c) ? c : 0);

            Assert.AreEqual(
                roster,
                total,
                $"{name}: xenotypeCounts sums to {total} but the roster has {roster}."
            );
        }
    }
}
