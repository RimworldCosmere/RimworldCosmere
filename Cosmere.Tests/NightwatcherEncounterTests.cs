using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The Nightwatcher window spends a choice a pawn only gets once, so these guard the player
///     being able to see what they are spending it on. Source assertions, since Unity is absent here.
/// </summary>
[TestClass]
public class NightwatcherEncounterTests {
    private const string DialogPath = "System/Roshar/Dialog/Dialog_NightwatcherEncounter.cs";

    private const string BoonElement = "Cosmere.System.Roshar.Def.NightwatcherBoonDef";

    private static readonly Regex CroKeyReference = new Regex(@"""(CRO_[A-Za-z0-9_]+)""", RegexOptions.Compiled);

    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereRoshar", "Languages"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereRoshar/Languages above the test output directory.");

            return dir.FullName;
        }
    }

    private static string KeyedDir => Path.Combine(RepoRoot, "CosmereRoshar", "Languages", "English", "Keyed");

    private static string DialogSource => File.ReadAllText(Path.Combine(
        RepoRoot,
        "CosmereCore",
        "CosmereCore",
        DialogPath.Replace('/', Path.DirectorySeparatorChar)
    ));

    /// <summary>The source with comments stripped, so a comment naming a banned pattern is not read as one.</summary>
    private static string DialogCodeOnly() {
        string[] lines = DialogSource.Split('\n');
        List<string> kept = [];
        for (int i = 0; i < lines.Length; i++) {
            string trimmed = lines[i].TrimStart();
            if (trimmed.StartsWith("//") || trimmed.StartsWith("*")) continue;

            kept.Add(lines[i]);
        }

        return string.Join("\n", kept);
    }

    [TestMethod]
    public void ConfirmButtonIsDrawnUnconditionally() {
        string code = DialogCodeOnly();

        Assert.IsTrue(
            code.Contains("active: canConfirm"),
            "Confirm must always draw and grey out, not appear only once a boon is picked."
        );
        Assert.IsFalse(
            Regex.IsMatch(code, @"if\s*\(\s*canConfirm\s*\)"),
            "canConfirm must gate the button's active flag, never whether it is drawn."
        );
        Assert.IsTrue(
            code.Contains("CRO_NW_Boon_ConfirmNeedsBoon"),
            "A disabled confirm has to say why it is disabled."
        );
    }

    [TestMethod]
    public void NoBareImguiStateAssignments() {
        string code = DialogCodeOnly();

        foreach (string banned in new[] { "Text.Font =", "Text.Anchor =", "GUI.color =", "Text.WordWrap =" }) {
            Assert.IsFalse(
                code.Contains(banned),
                $"'{banned}' leaks into vanilla UI. Wrap the draw in a Verse.TextBlock instead."
            );
        }

        Assert.IsTrue(code.Contains("new TextBlock("), "The dialog should scope its font and colour with TextBlock.");
    }

    [TestMethod]
    public void OldNightwatcherKeyPrefixIsGone() {
        Assert.IsFalse(
            DialogSource.Contains("Cosmere_Roshar_NW_"),
            "Dialog keys were renamed to the CRO_ convention."
        );

        string keyed = File.ReadAllText(Path.Combine(KeyedDir, "Nightwatcher.xml"));
        Assert.IsFalse(keyed.Contains("Cosmere_Roshar_NW_"), "Keyed entries were renamed to the CRO_ convention.");
    }

    [TestMethod]
    public void EveryReferencedKeyIsDefined() {
        HashSet<string> defined = [];
        foreach (string file in Directory.EnumerateFiles(KeyedDir, "*.xml")) {
            foreach (XElement element in XDocument.Load(file).Root!.Elements()) {
                defined.Add(element.Name.LocalName);
            }
        }

        List<string> missing = CroKeyReference.Matches(DialogSource)
                                              .Select(m => m.Groups[1].Value)
                                              .Distinct()
                                              .Where(key => !defined.Contains(key))
                                              .ToList();

        Assert.AreEqual(
            0,
            missing.Count,
            "Undefined keys render as the key name in game: " + string.Join(", ", missing)
        );
    }

    [TestMethod]
    public void TierTabsPartitionEveryBoon() {
        string path = Path.Combine(RepoRoot, "CosmereRoshar", "Defs", "Nightwatcher", "BoonDefs.xml");
        List<int> tiers = XDocument.Load(path)
                                   .Root!
                                   .Elements(BoonElement)
                                   .Select(d => int.Parse(d.Element("powerTier")?.Value ?? "0"))
                                   .ToList();

        Assert.AreEqual(20, tiers.Count, "Twenty boons ship today; the tab counts below track that total.");
        Assert.AreEqual(8, tiers.Count(t => t == 1), "Tab 1 shows the minor boons.");
        Assert.AreEqual(7, tiers.Count(t => t == 2), "Tab 2 shows the moderate boons.");
        Assert.AreEqual(5, tiers.Count(t => t == 3), "Tab 3 shows the major boons.");
        Assert.AreEqual(20, tiers.Count(t => t is >= 1 and <= 3), "Every boon lands in exactly one of three tabs.");
    }

    [TestMethod]
    public void TierIsClampedSoNoBoonCanVanish() {
        Assert.IsTrue(
            DialogCodeOnly().Contains("Mathf.Clamp(boon.powerTier, 1, 3)"),
            "A def with a tier outside 1-3 must still show up under some tab."
        );
    }

    [TestMethod]
    public void SelectionPanelAndTabsSitInsideTheWindow() {
        string code = DialogCodeOnly();

        Assert.IsTrue(code.Contains("DrawSelectionPanel"), "The picked boon needs a pinned panel above the button.");
        Assert.IsTrue(code.Contains("DrawTierTabs"), "The tiers are tabs now, not inline headers in the scroll.");
        Assert.IsFalse(code.Contains("isNewTier"), "Inline tier headers were replaced by the tab strip.");
        Assert.IsTrue(code.Contains("inRect.yMax - FooterHeight"), "The footer must be derived from inRect, not fixed.");
        Assert.IsTrue(code.Contains("listRect.width - ScrollbarWidth"), "Scroll content has to leave room for the bar.");
    }

    /// <summary>
    ///     A raw float named arg renders rounded, so 0.2 reached the player as "+0" while whole
    ///     numbers like 29 looked correct and hid it.
    /// </summary>
    [TestMethod]
    public void EveryNumericEffectArgIsFormattedBeforeItIsNamed() {
        string dialog = DialogSource;
        Regex raw = new Regex(
            @"^\s*\(?[A-Za-z_][A-Za-z0-9_.]*(\s*\*\s*[0-9.]+f)?\)?\.Named\(""(VALUE|AMOUNT|YEARS)""\)",
            RegexOptions.Multiline
        );

        List<string> offenders = [];
        foreach (Match match in raw.Matches(dialog)) {
            string line = match.Value.Trim();

            // an int has no fractional part to lose
            if (line.Contains("(int)", StringComparison.Ordinal)) continue;

            offenders.Add(line);
        }

        Assert.AreEqual(
            0,
            offenders.Count,
            "Call ToString(\"0.##\") first, the way the rest of the mod does:\n" + string.Join("\n", offenders)
        );
    }
}
