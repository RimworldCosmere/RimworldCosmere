using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     ReadingOutcomeDoerGainResearch.GetTopicRulePacks does
///     <c>values.Keys.Select(x =&gt; x.generalRules)</c> with no null filter, so a research project
///     without generalRules yields a null RulePack straight into Book.AppendDoerRules, which
///     dereferences it. The book dies mid-generation and takes the GenStep down with it.
/// </summary>
[TestClass]
public class BookTopicRulesTests {
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

    private static IEnumerable<string> DefFiles() {
        foreach (string mod in new[] { "CosmereCore", "CosmereScadrial", "CosmereRoshar" }) {
            string dir = Path.Combine(RepoRoot, mod, "Defs");
            if (!Directory.Exists(dir)) continue;

            foreach (string path in Directory.GetFiles(dir, "*.xml", SearchOption.AllDirectories)) {
                yield return path;
            }
        }
    }

    /// <summary>
    ///     A book whose doer rolls one of our defs has to be able to name its subject. Without
    ///     rulesStrings there is nothing to resolve and generation throws.
    /// </summary>
    [TestMethod]
    [DataRow("ResearchProjectDef")]
    [DataRow("SkillDef")]
    public void EveryBookTopicCanNameItself(string defType) {
        List<string> offenders = [];
        int seen = 0;

        foreach (string path in DefFiles()) {
            XElement? root;
            try {
                root = XDocument.Load(path).Root;
            } catch (global::System.Xml.XmlException e) {
                Assert.Fail($"{Path.GetRelativePath(RepoRoot, path)} is not valid XML: {e.Message}");
                return;
            }

            if (root == null) continue;

            foreach (XElement def in root.Descendants(defType)) {
                string? name = def.Element("defName")?.Value;
                if (name == null) continue;

                seen++;
                XElement? strings = def.Element("generalRules")?.Element("rulesStrings");
                bool hasSubject = false;
                if (strings != null) {
                    foreach (XElement li in strings.Elements("li")) {
                        if (li.Value.StartsWith("subject->", StringComparison.Ordinal)) {
                            hasSubject = true;
                            break;
                        }
                    }
                }

                if (!hasSubject) offenders.Add($"{name} ({Path.GetRelativePath(RepoRoot, path)})");
            }
        }

        Assert.IsTrue(seen > 0, $"Found no {defType}s at all - the walk is wrong, not the defs.");

        string detail = $"These {defType}s have no generalRules/rulesStrings 'subject->' entry, " +
            "so any book that rolls them throws a NullReferenceException during generation: " +
            string.Join(", ", offenders);

        Assert.AreEqual(0, offenders.Count, detail);
    }
}
