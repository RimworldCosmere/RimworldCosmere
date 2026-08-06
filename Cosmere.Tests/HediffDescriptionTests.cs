using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     HediffDef.description is printed verbatim on the health card - none of vanilla's 379 put
///     a grammar placeholder in one. Ours do, so they only read correctly under a hediff class
///     that resolves them; without one the player sees the raw {PAWN_nameDef} braces.
/// </summary>
[TestClass]
public class HediffDescriptionTests {
    private static readonly HashSet<string> ResolvingClasses = [
        "Cosmere.Core.Hediff.PawnDescribedHediff",
        "Cosmere.System.Roshar.Hediff.NightwatcherPassiveHediff",
        "Cosmere.System.Roshar.Hediff.NightwatcherBoonHediff",
        "Cosmere.System.Roshar.Hediff.NightwatcherCurseHediff",
    ];

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

    [TestMethod]
    public void EveryPlaceholderDescriptionHasAClassThatResolvesIt() {
        Dictionary<string, XElement> abstracts = [];
        List<XElement> defs = [];

        foreach (string mod in new[] { "CosmereCore", "CosmereScadrial", "CosmereRoshar" }) {
            string dir = Path.Combine(RepoRoot, mod, "Defs");
            if (!Directory.Exists(dir)) continue;

            foreach (string path in Directory.GetFiles(dir, "*.xml", SearchOption.AllDirectories)) {
                XElement? root = XDocument.Load(path).Root;
                if (root == null) continue;

                foreach (XElement def in root.Descendants("HediffDef")) {
                    string? asName = (string?)def.Attribute("Name");
                    if (asName != null) abstracts[asName] = def;
                    defs.Add(def);
                }
            }
        }

        Assert.IsTrue(defs.Count > 0, "Found no HediffDefs at all - the walk is wrong, not the defs.");

        string? ClassOf(XElement def, int depth) {
            if (depth > 8) return null;
            string? own = def.Element("hediffClass")?.Value.Trim();
            if (own != null) return own;

            string? parent = (string?)def.Attribute("ParentName");
            return parent != null && abstracts.TryGetValue(parent, out XElement? p) ? ClassOf(p, depth + 1) : null;
        }

        List<string> offenders = [];
        foreach (XElement def in defs) {
            if ((string?)def.Attribute("Abstract") == "True") continue;

            string? description = def.Element("description")?.Value;
            if (description == null || !description.Contains("{PAWN_", StringComparison.Ordinal)) continue;

            string? cls = ClassOf(def, 0);
            if (cls == null || !ResolvingClasses.Contains(cls)) {
                offenders.Add($"{def.Element("defName")?.Value} (class {cls ?? "none"})");
            }
        }

        string detail = "These hediffs put a {PAWN_...} placeholder in their description but their " +
            "hediffClass does not resolve it, so the player reads the raw braces: " +
            string.Join(", ", offenders);

        Assert.AreEqual(0, offenders.Count, detail);
    }

    /// <summary>
    ///     The health tab lists hediffs by label. Two that share one are indistinguishable - a
    ///     Twinborn savant in tin showed "tin savant" twice with no way to tell which art was
    ///     which.
    /// </summary>
    [TestMethod]
    public void NoTwoHediffsShareALabel() {
        Dictionary<string, List<string>> byLabel = [];

        foreach (string mod in new[] { "CosmereCore", "CosmereScadrial", "CosmereRoshar" }) {
            string dir = Path.Combine(RepoRoot, mod, "Defs");
            if (!Directory.Exists(dir)) continue;

            foreach (string path in Directory.GetFiles(dir, "*.xml", SearchOption.AllDirectories)) {
                XElement? root = XDocument.Load(path).Root;
                if (root == null) continue;

                foreach (XElement def in root.Descendants("HediffDef")) {
                    string? name = def.Element("defName")?.Value;
                    string? label = def.Element("label")?.Value.Trim();
                    if (name == null || label == null) continue;

                    if (!byLabel.TryGetValue(label, out List<string>? names)) {
                        names = [];
                        byLabel[label] = names;
                    }

                    names.Add(name);
                }
            }
        }

        Assert.IsTrue(byLabel.Count > 0, "Found no labelled HediffDefs - the walk is wrong, not the defs.");

        List<string> collisions = [];
        foreach (KeyValuePair<string, List<string>> pair in byLabel) {
            if (pair.Value.Count > 1) collisions.Add($"'{pair.Key}' ({string.Join(", ", pair.Value)})");
        }

        string detail = "These hediffs share a label, so the health tab shows identical rows the " +
            "player cannot tell apart: " + string.Join("; ", collisions);

        Assert.AreEqual(0, collisions.Count, detail);
    }
}
