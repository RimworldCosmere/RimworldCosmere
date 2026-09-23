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
public class AutocastRuleEditorTests {
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

        foreach (string name in Enum.GetNames(typeof(AutocastComparison))) {
            Assert.IsTrue(
                Regex.IsMatch(dialog, $@"AutocastComparison\.{name} => ""[<>=]"","),
                $"{name} has no comparison glyph."
            );

            string key = $"CC_Autocast_Comparison_{name}_Tip";
            Assert.IsTrue(dialog.Contains($@"""{key}"".Translate()"), $"{name} has no tooltip.");
            Assert.IsTrue(keyed.Contains($"<{key}>"), $"{key} is referenced but not defined.");
        }
    }

    [TestMethod]
    public void NoLabelFallsBackToTheEnumName() {
        Assert.IsFalse(
            Regex.IsMatch(DialogSource, @"_ =>\s*\w+\.ToString\(\)"),
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

        Assert.IsTrue(source.Contains("return target < Idle(kind);"), "Tapping has to read as below rest.");
        Assert.IsTrue(
            source.Contains("? idle - clamped * (idle - Min(kind))"),
            "Tapping has to move the target down from rest."
        );
        Assert.IsTrue(
            source.Contains(": idle + clamped * (Max(kind) - idle)"),
            "Storing has to move the target up from rest."
        );
    }

    /// <summary>
    ///     Intensity is the other half of the round trip: it has to measure travel from rest, in
    ///     whichever direction the target sits, or the slider jumps the moment direction changes.
    /// </summary>
    [TestMethod]
    public void IntensityIsMeasuredFromRestInBothDirections() {
        string source = DialRangeSource;

        Assert.IsTrue(source.Contains("float span = target < idle ? idle - Min(kind) : Max(kind) - idle;"));
        Assert.IsTrue(source.Contains("Mathf.Clamp01(Mathf.Abs(target - idle) / span)"));
    }

    /// <summary>
    ///     Two directions and three comparisons both fit in their row, so they are inline buttons
    ///     now. The two menus left are the trigger kind and the release, whose options are
    ///     sentences rather than words.
    /// </summary>
    [TestMethod]
    public void DirectionAndComparisonNoLongerOpenAMenu() {
        string dialog = DialogSource;

        Assert.AreEqual(2, Regex.Matches(dialog, @"new FloatMenu\(").Count);
        Assert.IsTrue(dialog.Contains("DrawDirectionButtons("), "The dial direction should be inline buttons.");
        Assert.IsTrue(dialog.Contains("DrawComparisonButtons("), "The comparison should be inline buttons.");
    }

    [TestMethod]
    public void TheEmptyTriggerListSaysSomething() {
        Assert.IsTrue(DialogSource.Contains(@"""CC_Autocast_Editor_NoTriggers"".Translate()"));
        Assert.IsTrue(KeyedSource.Contains("<CC_Autocast_Editor_NoTriggers>"));
    }

    /// <summary>
    ///     The runner stops at the first trigger that fails, so the header has to say all of them.
    /// </summary>
    [TestMethod]
    public void TheHeaderStatesTheAndSemantics() {
        Assert.IsTrue(
            Regex.IsMatch(
                KeyedSource,
                @"<CC_Autocast_Editor_TriggersHeader>[^<]*&lt;b&gt;all&lt;/b&gt;"
            ),
            "The triggers header has to tell the player every trigger must pass."
        );
    }

    private static void AssertLabelled<T>(string typeName)
        where T : struct, Enum {
        string dialog = DialogSource;
        string keyed = KeyedSource;

        foreach (string name in Enum.GetNames(typeof(T))) {
            Match arm = Regex.Match(dialog, $@"{typeName}\.{name} => ""(CC_\w+)""\.Translate\(\)");
            Assert.IsTrue(arm.Success, $"{typeName}.{name} has no keyed label.");
            Assert.IsTrue(
                keyed.Contains($"<{arm.Groups[1].Value}>"),
                $"{arm.Groups[1].Value} is referenced but not defined."
            );
        }
    }
}
