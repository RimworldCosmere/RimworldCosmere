using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     PawnApparelGenerator dresses a pawn from its kind's apparelTags within an apparelMoney
///     budget. A kind that declares neither, and inherits neither, generates naked.
/// </summary>
[TestClass]
public class PawnKindApparelTests {
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

    private static List<XElement> PlayerPawnKinds() {
        string path = Path.Combine(RepoRoot, "CosmereScadrial", "Defs", "Races", "PawnKinds.xml");
        Assert.IsTrue(File.Exists(path), $"Expected pawn kinds at {path}");

        List<XElement> kinds = new List<XElement>();
        foreach (XElement kind in XDocument.Load(path).Root!.Elements("PawnKindDef")) {
            kinds.Add(kind);
        }

        Assert.IsTrue(kinds.Count > 0, "Expected at least one pawn kind.");
        return kinds;
    }

    [TestMethod]
    public void EveryPlayerPawnKindCanBeClothed() {
        List<string> naked = new List<string>();

        List<XElement> kinds = PlayerPawnKinds();

        foreach (XElement kind in kinds) {
            string defName = kind.Element("defName")?.Value ?? "(unnamed)";

            if (Clothed(kind, kinds)) continue;

            naked.Add(defName);
        }

        Assert.AreEqual(
            0,
            naked.Count,
            $"These kinds have no apparel budget or tags, so they generate naked: {string.Join(", ", naked)}"
        );
    }

    /// <summary>
    ///     Whether this kind ends up dressed, following ParentName as far as it goes in this file.
    /// </summary>
    /// <remarks>
    ///     A kind that names a parent inherits the parent's apparel, so reading the def on its own
    ///     reports every derived kind as naked. The raid koloss tiers are three of them - they
    ///     carry only what differs from the kind they descend from.
    /// </remarks>
    private static bool Clothed(XElement? kind, List<XElement> all) {
        while (kind != null) {
            bool hasTags = kind.Element("apparelTags")?.Elements("li") is { } tags && HasAny(tags);
            bool hasMoney = !string.IsNullOrWhiteSpace(kind.Element("apparelMoney")?.Value);
            if (hasTags && hasMoney) return true;

            string? parent = kind.Attribute("ParentName")?.Value;
            kind = parent == null
                ? null
                : all.FirstOrDefault(other => other.Attribute("Name")?.Value == parent);
        }

        return false;
    }

    [TestMethod]
    public void ApparelBudgetsRankByStation() {
        Dictionary<string, int> lowerBound = new Dictionary<string, int>();

        foreach (XElement kind in PlayerPawnKinds()) {
            string? defName = kind.Element("defName")?.Value;
            string? money = kind.Element("apparelMoney")?.Value;
            if (defName == null || money == null) continue;

            lowerBound[defName] = int.Parse(money.Split('~')[0]);
        }

        int noble = lowerBound["Cosmere_Scadrial_PawnKind_Noble"];
        int scadrian = lowerBound["Cosmere_Scadrial_PawnKind_Scadrian"];
        int terris = lowerBound["Cosmere_Scadrial_PawnKind_Terris"];
        int skaa = lowerBound["Cosmere_Scadrial_PawnKind_Skaa"];

        Assert.IsTrue(noble > scadrian, "Nobles should be dressed better than ordinary scadrians.");
        Assert.IsTrue(scadrian > terris, "Terris stewards are kept, not wealthy.");
        Assert.IsTrue(terris > skaa, "Skaa are the poorest of the Final Empire.");
    }

    private static bool HasAny(IEnumerable<XElement> elements) {
        foreach (XElement _ in elements) {
            return true;
        }

        return false;
    }
}
