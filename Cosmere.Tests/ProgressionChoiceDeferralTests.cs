using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     A ChoiceAction opens a window and returns; it does not block. Anything sitting after it in
///     the same action list used to run immediately, so an arc handed itself off while its own
///     campaign-defining fork was still on screen unanswered.
/// </summary>
[TestClass]
public class ProgressionChoiceDeferralTests {
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

    private static string ProgressionDirectory =>
        Path.Combine(RepoRoot, "CosmereScadrial", "Defs", "ScenarioProgression");

    /// <summary>
    ///     Every fork must offer a real pick, or the campaign silently resolves it. Two branches
    ///     with no labels is a dialog with two blank rows.
    /// </summary>
    [TestMethod]
    public void EveryChoiceNamesBothBranches() {
        if (!Directory.Exists(ProgressionDirectory)) return;

        List<string> offenders = [];
        int seen = 0;

        foreach (string path in Directory.GetFiles(ProgressionDirectory, "*.xml")) {
            XElement? root = XDocument.Load(path).Root;
            if (root == null) continue;

            foreach (XElement action in root.Descendants("li")) {
                string? cls = (string?)action.Attribute("Class");
                if (cls == null || !cls.EndsWith("ChoiceAction", StringComparison.Ordinal)) continue;

                seen++;
                foreach (string field in new[] { "titleKey", "textKey", "acceptKey", "declineKey" }) {
                    if (action.Element(field) == null) {
                        offenders.Add($"{Path.GetFileName(path)}: a ChoiceAction has no <{field}>");
                    }
                }

                if (action.Element("onAccept") == null || action.Element("onDecline") == null) {
                    offenders.Add($"{Path.GetFileName(path)}: a ChoiceAction is missing a branch");
                }
            }
        }

        Assert.IsTrue(seen > 0, "Found no ChoiceActions at all - the walk is wrong, not the defs.");
        Assert.AreEqual(0, offenders.Count, string.Join("; ", offenders));
    }

    /// <summary>
    ///     A fork that ends the campaign has to look different from one that does not before it
    ///     is clicked. Both branches need their hover text.
    /// </summary>
    [TestMethod]
    public void EveryChoiceExplainsBothBranchesOnHover() {
        if (!Directory.Exists(ProgressionDirectory)) return;

        List<string> offenders = [];

        foreach (string path in Directory.GetFiles(ProgressionDirectory, "*.xml")) {
            XElement? root = XDocument.Load(path).Root;
            if (root == null) continue;

            foreach (XElement action in root.Descendants("li")) {
                string? cls = (string?)action.Attribute("Class");
                if (cls == null || !cls.EndsWith("ChoiceAction", StringComparison.Ordinal)) continue;

                string title = action.Element("titleKey")?.Value ?? "(untitled)";
                foreach (string field in new[] { "acceptTipKey", "declineTipKey" }) {
                    if (action.Element(field) == null) {
                        offenders.Add($"{Path.GetFileName(path)}: '{title}' has no <{field}>");
                    }
                }
            }
        }

        Assert.AreEqual(0, offenders.Count, string.Join("; ", offenders));
    }

    /// <summary>
    ///     Two forks side by side in one action list both opened at once, and the second landed
    ///     on top. The player answered the visible one and the buried one was never resolved -
    ///     which is how the Well's lerasium question vanished. Nest them instead, so the order
    ///     is deliberate.
    /// </summary>
    [TestMethod]
    public void NoTwoChoicesAreSiblings() {
        if (!Directory.Exists(ProgressionDirectory)) return;

        List<string> offenders = [];

        foreach (string path in Directory.GetFiles(ProgressionDirectory, "*.xml")) {
            XElement? root = XDocument.Load(path).Root;
            if (root == null) continue;

            foreach (XElement list in root.Descendants()) {
                if (list.Name.LocalName is not ("actions" or "onAccept" or "onDecline" or "onMissing")) continue;

                int choices = 0;
                foreach (XElement child in list.Elements("li")) {
                    string? cls = (string?)child.Attribute("Class");
                    if (cls != null && cls.EndsWith("ChoiceAction", StringComparison.Ordinal)) choices++;
                }

                if (choices > 1) {
                    offenders.Add($"{Path.GetFileName(path)}: <{list.Name.LocalName}> holds {choices} ChoiceActions");
                }
            }
        }

        string detail = "These action lists put two forks side by side, so both dialogs open and " +
            "the player only sees the top one: " + string.Join("; ", offenders);

        Assert.AreEqual(0, offenders.Count, detail);
    }
}
