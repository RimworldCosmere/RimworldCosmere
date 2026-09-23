using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The dock has one button surface. State rides on the label, so a kind enum that picked a
///     fill colour is the thing this guards against coming back.
/// </summary>
[TestClass]
public class DockButtonTests {
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

    private static string Core(params string[] parts) => File.ReadAllText(Path.Combine(
        [RepoRoot, "CosmereCore", "CosmereCore", .. parts]
    ));

    /// <summary>The file with comments stripped, so prose explaining a banned pattern is not read as one.</summary>
    private static string CodeOnly(params string[] parts) {
        string[] lines = Core(parts).Split('\n');
        List<string> kept = [];

        for (int i = 0; i < lines.Length; i++) {
            string trimmed = lines[i].TrimStart();
            if (trimmed.StartsWith("//") || trimmed.StartsWith("*")) continue;

            kept.Add(lines[i]);
        }

        return string.Join("\n", kept);
    }

    [TestMethod]
    public void ButtonHasNoKindsAndNoAccentFill() {
        string source = CodeOnly("Core", "UI", "Dock", "DockButton.cs");

        Assert.IsFalse(source.Contains("DockButtonKind"), "Buttons have one surface; there are no kinds.");
        Assert.IsFalse(source.Contains("Lighten("), "Nothing lightens an accent into a fill any more.");
        Assert.IsFalse(source.Contains("new Color(accent.r"), "No accent-derived fill or border.");
    }

    [TestMethod]
    public void AbilityIconsDrawAtTwentyPixels() {
        string source = CodeOnly("Core", "UI", "Dock", "DockButton.cs");

        Assert.IsTrue(source.Contains("IconSize = 20f"), "Aura icons and bendalloy stop reading below 20px.");
    }

    [TestMethod]
    public void AbilityRowsDrawTheAbilityIcon() {
        string source = CodeOnly("System", "Scadrial", "UI", "AllomancyDockSection.cs");

        Assert.IsTrue(source.Contains("uiIcon"), "An ability row is named by its own icon.");
    }

    /// <summary>
    ///     GenText.Truncate is not tag-aware, so a bold label cut mid-tag renders "Steelsight</".
    /// </summary>
    [TestMethod]
    public void BoldIsAppliedAfterTruncationNotBefore() {
        Assert.IsFalse(
            CodeOnly("Core", "UI", "Dock", "DockButton.cs").Contains("<b>"),
            "The button asks for bold; it never builds the tag itself."
        );
        Assert.IsFalse(
            CodeOnly("System", "Roshar", "UI", "SurgebindingDockSection.cs").Contains("<b>"),
            "A running ability row asks for bold; it never builds the tag itself."
        );
        Assert.IsTrue(
            CodeOnly("Core", "UI", "UIText.cs").Contains("bool bold"),
            "EllipsisLabel owns the tag, so it can wrap the string after it fits."
        );
    }

    [TestMethod]
    public void EllipsisLabelDoesNotWrap() {
        Assert.IsTrue(
            CodeOnly("Core", "UI", "UIText.cs").Contains("Text.WordWrap = false"),
            "An ellipsised label that wraps gets its second line clipped by the row."
        );
    }
}
