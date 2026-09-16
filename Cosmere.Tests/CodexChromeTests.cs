using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The Codex chrome is the frame every system's page is drawn inside, so a regression here is
///     one the player meets on every pawn.
/// </summary>
[TestClass]
public class CodexChromeTests {
    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereCore", "Languages"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereCore/Languages above the test output directory.");

            return dir.FullName;
        }
    }

    private static string Codex(string file) => File.ReadAllText(Path.Combine(
        RepoRoot, "CosmereCore", "CosmereCore", "Core", "UI", "Codex", file
    ));

    /// <summary>
    ///     Every key defined anywhere under the Keyed folder. The files there are merged by the
    ///     game, so which one a key lives in is not something a test should care about.
    /// </summary>
    private static HashSet<string> DefinedKeys() {
        HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);
        string dir = Path.Combine(RepoRoot, "CosmereCore", "Languages", "English", "Keyed");

        foreach (string file in Directory.GetFiles(dir, "*.xml", SearchOption.AllDirectories)) {
            foreach (XElement element in XDocument.Load(file).Root!.Descendants()) {
                keys.Add(element.Name.LocalName);
            }
        }

        return keys;
    }

    /// <summary>
    ///     A 20px target at the frame edge is a miss waiting to happen, and the row had three
    ///     different sizes on it.
    /// </summary>
    [TestMethod]
    public void EveryTargetOnAnAutocastRowIsTheSameSizeAndNoSmallerThanTwentyFour() {
        string source = Codex("AutocastSubtabRenderer.cs");

        Assert.IsTrue(
            source.Contains("private const float TargetSize = 24f;"),
            "One constant decides how big a target is."
        );

        foreach (string target in new[] { "Rect add", "Rect enabled", "Rect remove", "Rect edit" }) {
            Match rect = Regex.Match(source, target + @" = new Rect\([^;]+;");
            Assert.IsTrue(rect.Success, $"{target} is not where this test expects it.");
            Assert.IsTrue(
                rect.Value.Contains("TargetSize)"),
                $"{target} must take its height from TargetSize."
            );
        }

        Assert.IsFalse(
            Regex.IsMatch(source, @"\b(?:1[0-9]|2[0-3])f,\s*(?:1[0-9]|2[0-3])f\)"),
            "No rect in this file may be sized under 24px."
        );
    }

    /// <summary>
    ///     HeaderFill, the alternating stripe and the mouseover highlight all landed on the same
    ///     rows, which is three greys arguing over one row of text.
    /// </summary>
    [TestMethod]
    public void AutocastRowsAreNotStriped() {
        string source = Codex("AutocastSubtabRenderer.cs");

        Assert.IsFalse(source.Contains("striped"), "The stripe is gone; the header fill separates groups.");
        Assert.IsFalse(source.Contains("0.03f"), "And its colour with it.");
        Assert.IsTrue(source.Contains("Widgets.DrawHighlightIfMouseover(row)"), "Hover still has to read.");
    }

    /// <summary>
    ///     A rule takes a whole dialog to build and went away on one click with no undo.
    /// </summary>
    [TestMethod]
    public void RemovingARuleAsksFirst() {
        string source = Codex("AutocastSubtabRenderer.cs");

        // CreateConfirmation(TaggedString, Action, bool destructive, ...) - Verse.Dialog_MessageBox, 1.6.4633.
        Assert.IsTrue(
            source.Contains("Dialog_MessageBox.CreateConfirmation("),
            "Removal goes through RimWorld's own confirmation."
        );
        Assert.IsTrue(
            Regex.IsMatch(source, @"\(\) => store\.RemoveRule\(pawn, rule\)"),
            "And only happens once the player has said yes."
        );
        Assert.AreEqual(
            1,
            Regex.Matches(source, @"RemoveRule\(").Count,
            "A second removal path would route around the confirm."
        );
    }

    /// <summary>
    ///     Nothing on the row said what the checkbox did.
    /// </summary>
    [TestMethod]
    public void TheEnableCheckboxSaysWhatItToggles() {
        string source = Codex("AutocastSubtabRenderer.cs");

        Assert.IsTrue(Regex.IsMatch(
            source,
            @"TooltipHandler\.TipRegion\(enabled, ""CC_Codex_Autocast_EnabledTip"""
        ));
    }

    /// <summary>
    ///     Selected and unselected were the same hue at two brightnesses, so the underline was
    ///     doing all the work. Both stay warm; the unselected one just stops hiding.
    /// </summary>
    [TestMethod]
    public void AnUnselectedSubtabIsStillReadable() {
        string source = Codex("SubtabBar.cs");

        Match unselected = Regex.Match(
            source,
            @"UnselectedText = new Color\((?<r>[0-9.]+)f, (?<g>[0-9.]+)f, (?<b>[0-9.]+)f\)"
        );
        Assert.IsTrue(unselected.Success, "The unselected tone is a named constant.");

        float r = float.Parse(unselected.Groups["r"].Value, CultureInfo.InvariantCulture);
        float g = float.Parse(unselected.Groups["g"].Value, CultureInfo.InvariantCulture);
        float b = float.Parse(unselected.Groups["b"].Value, CultureInfo.InvariantCulture);

        Assert.IsTrue(r > 0.65f, "0.55 red read as disabled against the panel.");
        Assert.IsTrue(r > g && g > b, "Warm: it stays in the same family as selected.");

        Assert.IsTrue(
            source.Contains("isSelected ? SelectedText : UnselectedText"),
            "Both tones are named constants, not inlined in the draw."
        );

        // an accent tinted by alpha is fine; a literal tone inside the draw is not.
        string draw = source[source.IndexOf("public static bool Draw<T>", StringComparison.Ordinal)..];
        Assert.IsFalse(
            Regex.IsMatch(draw, @"new Color\(\d"),
            "No literal colour is built inside a draw method."
        );
    }

    /// <summary>
    ///     A tab with no tooltip is a word with no explanation, and the bar is the only way into
    ///     three of the four pages.
    /// </summary>
    [TestMethod]
    public void EverySubtabExplainsItself() {
        string bar = Codex("SubtabBar.cs");

        Assert.IsTrue(
            bar.Contains("TooltipHandler.TipRegion(tab, (tabs[i].labelKey + \"_Tip\").Translate())"),
            "The tip key is derived from the label key."
        );

        HashSet<string> defined = DefinedKeys();
        foreach (string file in new[] { "SubtabBar.cs", "ConnectionSubtab.cs" }) {
            foreach (Match label in Regex.Matches(Codex(file), @"""(CC_(?:Codex_Subtab|Connection_Tab)_[A-Za-z]+)""")) {
                string tip = label.Groups[1].Value + "_Tip";
                Assert.IsTrue(defined.Contains(tip), $"{tip} is referenced by the bar but never defined.");
            }
        }
    }

    /// <summary>
    ///     A key that resolves to nothing prints its own name on screen, so every key these files
    ///     name has to exist somewhere in the Keyed folder.
    /// </summary>
    [TestMethod]
    public void EveryKeyTheCodexChromeNamesIsDefined() {
        HashSet<string> defined = DefinedKeys();

        foreach (string file in new[] { "AutocastSubtabRenderer.cs", "SubtabBar.cs", "CodexChrome.cs" }) {
            foreach (Match key in Regex.Matches(Codex(file), @"""(CC_[A-Za-z0-9_]+)""")) {
                Assert.IsTrue(defined.Contains(key.Groups[1].Value), $"{key.Groups[1].Value} is not defined.");
            }
        }
    }

    /// <summary>
    ///     Flat white on a dark box is the templated mod panel tell. The title sits on parchment,
    ///     and a skin that wants its own tone passes HeaderTextColor in.
    /// </summary>
    [TestMethod]
    public void TheCodexTitleIsNotFlatWhite() {
        string source = Codex("CodexChrome.cs");

        Assert.IsFalse(source.Contains("Color.white"), "The title is not flat white.");
        Assert.IsTrue(
            source.Contains("DrawHeader(Rect rect, string label, Color accent, Color? textColor = null)"),
            "A skin can pass HeaderTextColor in."
        );
        Assert.IsTrue(source.Contains("textColor ?? TitleText"), "And parchment is what it falls back to.");
        Assert.IsFalse(source.Contains("DrawBoxSolid(rect, new Color(0.08f"), "The header fill is a constant.");
    }
}
