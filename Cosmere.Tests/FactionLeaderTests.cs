using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Faction.TryGenerateNewLeader draws only from Combat pawn group options whose kind has
///     factionLeader, plus fixedLeaderKinds. A faction with neither logs "Faction leader for X
///     is null" on repeat from FactionTick, forever.
/// </summary>
[TestClass]
public class FactionLeaderTests {
    private static readonly HashSet<string> LeaderKinds = ["PirateBoss", "Town_Councilman", "Tribal_ChiefMelee"];

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

    private static Dictionary<string, XElement> abstracts = [];

    private static XElement? Inherited(XElement def, string tag, int depth = 0) {
        if (depth > 8) return null;
        XElement? own = def.Element(tag);
        if (own != null) return own;

        string? parent = (string?)def.Attribute("ParentName");
        return parent != null && abstracts.TryGetValue(parent, out XElement? p) ? Inherited(p, tag, depth + 1) : null;
    }

    private static string? InheritedText(XElement def, string tag, int depth = 0) {
        return Inherited(def, tag, depth)?.Value.Trim();
    }

    /// <summary>Every faction we ship that vanilla expects a leader for must be able to make one.</summary>
    [TestMethod]
    public void EveryFactionCanProduceALeader() {
        List<XElement> defs = [];
        abstracts = [];

        foreach (string mod in new[] { "CosmereCore", "CosmereScadrial", "CosmereRoshar" }) {
            string dir = Path.Combine(RepoRoot, mod, "Defs");
            if (!Directory.Exists(dir)) continue;

            foreach (string path in Directory.GetFiles(dir, "*.xml", SearchOption.AllDirectories)) {
                XElement? root = XDocument.Load(path).Root;
                if (root == null) continue;

                foreach (XElement def in root.Descendants("FactionDef")) {
                    string? asName = (string?)def.Attribute("Name");
                    if (asName != null) abstracts[asName] = def;
                    defs.Add(def);
                }
            }
        }

        Assert.IsTrue(defs.Count > 0, "Found no FactionDefs at all - the walk is wrong, not the defs.");

        List<string> offenders = [];
        foreach (XElement def in defs) {
            if ((string?)def.Attribute("Abstract") == "True") continue;

            // ShouldHaveLeader: humanlike, not hidden, not the player, not temporary.
            if ((InheritedText(def, "humanlikeFaction") ?? "true").ToLowerInvariant() != "true") continue;
            if ((InheritedText(def, "hidden") ?? "false").ToLowerInvariant() == "true") continue;
            if ((InheritedText(def, "isPlayer") ?? "false").ToLowerInvariant() == "true") continue;
            if (Inherited(def, "fixedLeaderKinds") != null) continue;

            bool canLead = false;
            XElement? makers = Inherited(def, "pawnGroupMakers");
            if (makers != null) {
                foreach (XElement maker in makers.Elements("li")) {
                    if (maker.Element("kindDef")?.Value.Trim() != "Combat") continue;

                    XElement? options = maker.Element("options");
                    if (options == null) continue;
                    foreach (XElement option in options.Elements()) {
                        if (LeaderKinds.Contains(option.Name.LocalName)) canLead = true;
                    }
                }
            }

            if (!canLead) offenders.Add(def.Element("defName")?.Value ?? "(unnamed)");
        }

        string detail = "These factions have no fixedLeaderKinds and no factionLeader kind in a Combat " +
            "pawn group, so FactionTick logs 'Faction leader for X is null' every time it runs: " +
            string.Join(", ", offenders);

        Assert.AreEqual(0, offenders.Count, detail);
    }
}
