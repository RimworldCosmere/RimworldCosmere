using System;
using System.IO;
using System.Text.RegularExpressions;
using Cosmere.Core.Ability.Autocast;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Covers the autocast rule editor's labels and the dial direction it writes back.
/// </summary>
/// <remarks>
///     The dialog calls Translate and the dial range calls Mathf, and the test host has neither
///     Assembly-CSharp nor UnityEngine, so both are checked against their source instead.
/// </remarks>
[TestClass]
public partial class AutocastRuleEditorTests {
    private static string DialogSource => ReadSource(
        Path.Combine("CosmereCore", "CosmereCore", "Core", "Ability", "Autocast", "AutocastRuleEditorDialog.cs")
    );

    private static string DialRangeSource => ReadSource(
        Path.Combine("CosmereCore", "CosmereCore", "Core", "Ability", "Autocast", "AutocastDialRange.cs")
    );

    private static string KeyedSource => ReadSource(
        Path.Combine("CosmereCore", "Languages", "English", "Keyed", "Inspector.xml")
    );

    private static string ReadSource(string relativePath) {
        DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
            dir = dir.Parent;
        }

        Assert.IsNotNull(dir, "Could not locate CosmereScadrial/Defs above the test output directory.");

        return File.ReadAllText(Path.Combine(dir.FullName, relativePath));
    }

    /// <summary>
    ///     A missing arm used to fall through to ToString, which printed the C# enum name at the
    ///     player. Every member needs an arm, and every arm needs a key that actually exists.
    /// </summary>
    [TestMethod]
    public void EveryEnumMemberHasAKeyedLabel() {
        AssertLabelled<AutocastTriggerKind>("AutocastTriggerKind");
        AssertLabelled<AutocastRelease>("AutocastRelease");
    }

    [TestMethod]
    public void EveryComparisonHasAGlyphAndATooltip() {
        string dialog = DialogSource;
        string keyed = KeyedSource;

        foreach (string name in Enum.GetNames<AutocastComparison>()) {
            Assert.IsTrue(
                Regex.IsMatch(dialog, $@"AutocastComparison\.{name} => ""[<>=]"","),
                $"{name} has no comparison glyph."
            );

            string key = $"CC_Autocast_Comparison_{name}_Tip";
            Assert.Contains($@"""{key}"".Translate()", dialog, $"{name} has no tooltip.");
            Assert.Contains($"<{key}>", keyed, $"{key} is referenced but not defined.");
        }
    }

    [TestMethod]
    public void NoLabelFallsBackToTheEnumName() {
        Assert.IsFalse(
            ToStringFallbackPattern.IsMatch(DialogSource),
            "A label fell back to ToString, which shows the raw enum name in game."
        );
    }

    /// <summary>
    ///     The inline direction buttons write a target through TargetFor and read it back through
    ///     IsTapping. Swap either side of rest and the player's tap rule silently becomes a store.
    /// </summary>
    [TestMethod]
    public void TapAndStoreSitOnOppositeSidesOfRest() {
        string source = DialRangeSource;

        Assert.Contains("return target < Idle(kind);", source, "Tapping has to read as below rest.");
        Assert.Contains(
            "? idle - clamped * (idle - Min(kind))", source, "Tapping has to move the target down from rest."
        );
        Assert.Contains(
            ": idle + clamped * (Max(kind) - idle)", source, "Storing has to move the target up from rest."
        );
    }

    /// <summary>
    ///     Intensity is the other half of the round trip: it has to measure travel from rest, in
    ///     whichever direction the target sits, or the slider jumps the moment direction changes.
    /// </summary>
    [TestMethod]
    public void IntensityIsMeasuredFromRestInBothDirections() {
        string source = DialRangeSource;

        Assert.Contains("float span = target < idle ? idle - Min(kind) : Max(kind) - idle;", source);
        Assert.Contains("Mathf.Clamp01(Mathf.Abs(target - idle) / span)", source);
    }

    /// <summary>
    ///     Directions and comparisons are inline buttons. The two menus left are the trigger kind
    ///     and the release, whose options are sentences rather than words.
    /// </summary>
    [TestMethod]
    public void DirectionAndComparisonNoLongerOpenAMenu() {
        string dialog = DialogSource;

        Assert.HasCount(2, FloatMenuPattern.Matches(dialog));
        Assert.DoesNotContain("new FloatMenu(", MethodBody(dialog, "DrawDirectionButtons"));
        Assert.DoesNotContain("new FloatMenu(", MethodBody(dialog, "DrawComparisonButtons"));
        Assert.Contains("DrawDirectionButtons(", dialog, "The dial direction should be inline buttons.");
        Assert.Contains("DrawComparisonButtons(", dialog, "The comparison should be inline buttons.");
    }

    [GeneratedRegex(@"new FloatMenu\(")]
    private static partial Regex FloatMenuPattern { get; }

    private static string MethodBody(string source, string name) {
        Match method = Regex.Match(
            source,
            $@"\n    (?:private|public|internal|protected)[^\n]*\b{name}\(.*?(?=\n    (?:private|public|internal|protected)\b)",
            RegexOptions.Singleline
        );

        Assert.IsTrue(method.Success, $"{name} is gone from the dialog.");

        return method.Value;
    }

    [TestMethod]
    public void TheEmptyTriggerListSaysSomething() {
        Assert.Contains(@"""CC_Autocast_Editor_NoTriggers"".Translate()", DialogSource);
        Assert.Contains("<CC_Autocast_Editor_NoTriggers>", KeyedSource);
    }

    /// <summary>
    ///     The runner stops at the first trigger that fails, so the header has to say all of them.
    /// </summary>
    [TestMethod]
    public void TheHeaderStatesTheAndSemantics() {
        Assert.IsTrue(
            TriggersHeaderSaysAllPattern.IsMatch(KeyedSource),
            "The triggers header has to tell the player every trigger must pass."
        );
    }

    private static void AssertLabelled<T>(string typeName)
        where T : struct, Enum {
        string dialog = DialogSource;
        string keyed = KeyedSource;

        foreach (string name in Enum.GetNames<T>()) {
            Match arm = Regex.Match(dialog, $@"{typeName}\.{name} => ""(CC_\w+)""\.Translate\(\)");
            Assert.IsTrue(arm.Success, $"{typeName}.{name} has no keyed label.");
            Assert.Contains(
                $"<{arm.Groups[1].Value}>", keyed, $"{arm.Groups[1].Value} is referenced but not defined."
            );
        }
    }

    [GeneratedRegex(@"_ =>\s*\w+\.ToString\(\)")]
    private static partial Regex ToStringFallbackPattern { get; }

    [GeneratedRegex(@"<CC_Autocast_Editor_TriggersHeader>[^<]*&lt;b&gt;all&lt;/b&gt;")]
    private static partial Regex TriggersHeaderSaysAllPattern { get; }
}
