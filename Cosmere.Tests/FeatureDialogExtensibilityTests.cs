using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The features dialog builds its tabs from whatever CosmereFeatureDefs loaded, grouped by
///     the world each names. A new shard mod shipping its own Features.xml has to get a tab
///     without anyone editing Core.
/// </summary>
[TestClass]
public partial class FeatureDialogExtensibilityTests {
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

    private static string CoreDir => Path.Combine(RepoRoot, "CosmereCore", "CosmereCore");

    /// <summary>
    ///     Core must not name a shardworld to build the UI. The moment it does, adding Nalthis
    ///     means editing Core, which is the coupling the world defs exist to avoid.
    /// </summary>
    /// <summary>Comments may name a world as an example; code may not.</summary>
    [GeneratedRegex(@"^\s*(//|///|\*|/\*)")]
    private static partial Regex CommentLine { get; }

    [TestMethod]
    public void TheFeatureUiNamesNoWorld() {
        string[] uiFiles = [
            Path.Combine(CoreDir, "Core", "UI", "Dialog_CosmereFeatures.cs"),
            Path.Combine(CoreDir, "Core", "UI", "TabStrip.cs"),
            Path.Combine(CoreDir, "Core", "Util", "FeatureUtility.cs"),
        ];

        List<string> offenders = [];

        foreach (string path in uiFiles) {
            Assert.IsTrue(File.Exists(path), $"{Path.GetFileName(path)} is missing - was it moved?");

            string[] lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++) {
                if (CommentLine.IsMatch(lines[i])) continue;

                foreach (string world in new[] { "Scadrial", "Roshar", "Nalthis", "Taldain" }) {
                    if (lines[i].Contains(world, StringComparison.Ordinal)) {
                        offenders.Add($"{Path.GetFileName(path)}:{i + 1} names {world}");
                    }
                }
            }
        }

        string detail = "The feature UI must stay world-agnostic so a new shard mod gets a tab " +
            "for free: " + string.Join("; ", offenders);

        Assert.IsEmpty(offenders, detail);
    }

    /// <summary>Every world that ships features needs a tab, so it must resolve a skin.</summary>
    [TestMethod]
    public void EveryWorldWithFeaturesCanBeThemed() {
        Dictionary<string, XElement> worlds = [];
        List<XElement> features = [];

        foreach (string mod in new[] { "CosmereCore", "CosmereScadrial", "CosmereRoshar" }) {
            string dir = Path.Combine(RepoRoot, mod, "Defs");
            if (!Directory.Exists(dir)) continue;

            foreach (string path in Directory.GetFiles(dir, "*.xml", SearchOption.AllDirectories)) {
                XElement? root = XDocument.Load(path).Root;
                if (root == null) continue;

                foreach (XElement world in root.Descendants("Cosmere.Core.Def.CosmereWorldDef")) {
                    string? name = world.Element("defName")?.Value.Trim();
                    if (name != null) worlds[name] = world;
                }

                features.AddRange(root.Descendants("Cosmere.Core.Def.CosmereFeatureDef"));
            }
        }

        Assert.IsNotEmpty(features, "No features found - the walk is wrong, or none ship.");

        List<string> offenders = [];
        foreach (XElement feature in features) {
            string? worldName = feature.Element("world")?.Value.Trim();
            string featureName = feature.Element("defName")?.Value.Trim() ?? "(unnamed)";

            if (worldName == null) {
                offenders.Add($"{featureName} names no world");
                continue;
            }

            if (!worlds.ContainsKey(worldName)) {
                offenders.Add($"{featureName} names world '{worldName}', which no mod declares");
            }
        }

        Assert.IsEmpty(offenders, string.Join("; ", offenders));
    }
}
