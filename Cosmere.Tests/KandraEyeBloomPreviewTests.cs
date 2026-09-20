using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards the eye preview against drawing half of what a pawn draws. The cutout alone clamps
///     to white on a pale stone, which made steady and burning identical in the picker.
/// </summary>
[TestClass]
public class KandraEyeBloomPreviewTests {
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

    private static string Source(string file) {
        return File.ReadAllText(
            Path.Combine(RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Kandra", file)
        );
    }

    /// <summary>The bloom is a second additive pass on the same art, exactly as the glow node is.</summary>
    [TestMethod]
    public void ThePreviewDrawsTheBloomAndNotJustTheCutout() {
        string source = Source("Dialog_KandraForms.Eyes.cs");

        StringAssert.Contains(
            source,
            "ShaderDatabase.MoteGlow",
            "The preview no longer draws the bloom the pawn draws, so a lit eye reads as an unlit one."
        );

        StringAssert.Contains(
            source,
            "KandraAppearance.EyeLightStrengthFor(light) - 1f",
            "The bloom pass takes its strength from somewhere other than the light, so it cannot match the pawn."
        );
    }

    /// <summary>One drawer for both, so the preview can never drift from the picker chip again.</summary>
    [TestMethod]
    public void BothThePreviewAndThePickerGoThroughTheSameIrisDrawer() {
        StringAssert.Contains(
            Source("Dialog_KandraForms.cs"),
            "DrawIris(",
            "The body preview draws an iris by hand again instead of through the shared drawer."
        );

        StringAssert.Contains(
            Source("Dialog_KandraForms.Eyes.cs"),
            "DrawIris(",
            "The picker rows draw an iris by hand again instead of through the shared drawer."
        );
    }

    /// <summary>
    ///     A light row drawn as a flat colour cannot show a light: every strength clamps to the same
    ///     white on a pale stone.
    /// </summary>
    [TestMethod]
    public void TheLightRowsShowAnEyeRatherThanAFlatChip() {
        string source = Source("Dialog_KandraForms.Eyes.cs");

        Assert.IsFalse(
            source.Contains("ToHtmlStringRGB", StringComparison.Ordinal),
            "A light row is back to a computed hex chip, which reads identically for every strength."
        );

        StringAssert.Contains(
            source,
            "DrawEyeRow(row, labelKey.Translate(), name == designEyeLight",
            "The light rows no longer preview the eye they would make."
        );
    }
}
