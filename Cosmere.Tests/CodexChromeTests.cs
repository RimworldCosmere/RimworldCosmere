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
public partial class CodexChromeTests {
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

    private static string Codex(string file) {
        return File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "Core", "UI", "Codex", file
        ));
    }

    [GeneratedRegex(@"\b(?:1[0-9]|2[0-3])f,\s*(?:1[0-9]|2[0-3])f\)")]
    private static partial Regex UnderTwentyFourRect { get; }

    [GeneratedRegex(@"\(\) => store\.RemoveRule\(pawn, rule\)")]
    private static partial Regex ConfirmedRemoveRule { get; }

    [GeneratedRegex(@"RemoveRule\(")]
    private static partial Regex RemoveRuleCall { get; }

    [GeneratedRegex(@"TooltipHandler\.TipRegion\(enabled, ""CC_Codex_Autocast_EnabledTip""")]
    private static partial Regex EnabledCheckboxTip { get; }

    [GeneratedRegex(@"UnselectedText = new Color\((?<r>[0-9.]+)f, (?<g>[0-9.]+)f, (?<b>[0-9.]+)f\)")]
    private static partial Regex UnselectedTextColor { get; }

    [GeneratedRegex(@"new Color\(\d")]
    private static partial Regex InlineColorLiteral { get; }

    [GeneratedRegex(@"""(CC_(?:Codex_Subtab|Connection_Tab)_[A-Za-z]+)""")]
    private static partial Regex SubtabLabelKey { get; }

    [GeneratedRegex(@"""(CC_[A-Za-z0-9_]+)""")]
    private static partial Regex CoreKeyLiteral { get; }

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

        Assert.Contains(
            "private const float TargetSize = 24f;",
            source,
            "One constant decides how big a target is."
        );

        foreach (string target in new[] { "Rect add", "Rect enabled", "Rect remove", "Rect edit" }) {
            Match rect = Regex.Match(source, target + @" = new Rect\([^;]+;");
            Assert.IsTrue(rect.Success, $"{target} is not where this test expects it.");
            Assert.Contains(
                "TargetSize)",
                rect.Value,
                $"{target} must take its height from TargetSize."
            );
        }

        Assert.IsFalse(
            UnderTwentyFourRect.IsMatch(source),
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

        Assert.DoesNotContain("striped", source, "The stripe is gone; the header fill separates groups.");
        Assert.DoesNotContain("0.03f", source, "And its colour with it.");
        Assert.Contains("Widgets.DrawHighlightIfMouseover(row)", source, "Hover still has to read.");
    }

    /// <summary>
    ///     A rule takes a whole dialog to build and went away on one click with no undo.
    /// </summary>
    [TestMethod]
    public void RemovingARuleAsksFirst() {
        string source = Codex("AutocastSubtabRenderer.cs");

        // CreateConfirmation(TaggedString, Action, bool destructive, ...) - Verse.Dialog_MessageBox, 1.6.4633.
        Assert.Contains(
            "Dialog_MessageBox.CreateConfirmation(",
            source,
            "Removal goes through RimWorld's own confirmation."
        );
        Assert.IsTrue(
            ConfirmedRemoveRule.IsMatch(source),
            "And only happens once the player has said yes."
        );
        Assert.HasCount(
            1,
            RemoveRuleCall.Matches(source),
            "A second removal path would route around the confirm."
        );
    }

    /// <summary>
    ///     Nothing on the row said what the checkbox did.
    /// </summary>
    [TestMethod]
    public void TheEnableCheckboxSaysWhatItToggles() {
        string source = Codex("AutocastSubtabRenderer.cs");

        Assert.IsTrue(EnabledCheckboxTip.IsMatch(source));
    }

    /// <summary>
    ///     Selected and unselected were the same hue at two brightnesses, so the underline was
    ///     doing all the work. Both stay warm; the unselected one just stops hiding.
    /// </summary>
    [TestMethod]
    public void AnUnselectedSubtabIsStillReadable() {
        string source = Codex("SubtabBar.cs");

        Match unselected = UnselectedTextColor.Match(source);
        Assert.IsTrue(unselected.Success, "The unselected tone is a named constant.");

        float r = float.Parse(unselected.Groups["r"].Value, CultureInfo.InvariantCulture);
        float g = float.Parse(unselected.Groups["g"].Value, CultureInfo.InvariantCulture);
        float b = float.Parse(unselected.Groups["b"].Value, CultureInfo.InvariantCulture);

        Assert.IsGreaterThan(0.65f, r, "0.55 red read as disabled against the panel.");
        Assert.IsTrue(r > g && g > b, "Warm: it stays in the same family as selected.");

        Assert.Contains(
            "isSelected ? SelectedText : UnselectedText",
            source,
            "Both tones are named constants, not inlined in the draw."
        );

        // an accent tinted by alpha is fine; a literal tone inside the draw is not.
        string draw = source[source.IndexOf("public static bool Draw<T>", StringComparison.Ordinal)..];
        Assert.IsFalse(
            InlineColorLiteral.IsMatch(draw),
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

        Assert.Contains(
            "TooltipHandler.TipRegion(tab, (tabs[i].labelKey + \"_Tip\").Translate())",
            bar,
            "The tip key is derived from the label key."
        );

        HashSet<string> defined = DefinedKeys();
        foreach (string file in new[] { "SubtabBar.cs", "ConnectionSubtab.cs" }) {
            foreach (Match label in SubtabLabelKey.Matches(Codex(file))) {
                string tip = label.Groups[1].Value + "_Tip";
                Assert.Contains(tip, defined, $"{tip} is referenced by the bar but never defined.");
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
            foreach (Match key in CoreKeyLiteral.Matches(Codex(file))) {
                Assert.Contains(key.Groups[1].Value, defined, $"{key.Groups[1].Value} is not defined.");
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

        Assert.DoesNotContain("Color.white", source, "The title is not flat white.");
        Assert.Contains(
            "DrawHeader(Rect rect, string label, Color accent, Color? textColor = null)",
            source,
            "A skin can pass HeaderTextColor in."
        );
        Assert.Contains("textColor ?? TitleText", source, "And parchment is what it falls back to.");
        Assert.DoesNotContain("DrawBoxSolid(rect, new Color(0.08f", source, "The header fill is a constant.");
    }
}
