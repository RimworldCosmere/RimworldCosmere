using System;
using System.IO;
using System.Text.RegularExpressions;
using Cosmere.Core.Settings.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>Floats compared with == against an exact constant (S1244).</summary>
/// <remarks>No Verse here, so the game-bound ones name the defective construct in the source.</remarks>
[TestClass]
public class FloatEqualityDefectTests {
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

    private static string Core(params string[] parts) {
        string path = Path.Combine([RepoRoot, "CosmereCore", "CosmereCore", .. parts]);
        Assert.IsTrue(File.Exists(path), $"Expected a source file at {path}");

        return File.ReadAllText(path);
    }

    private static float NextUlp(float value, int steps) {
        return BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(value) + steps);
    }

    [TestMethod]
    public void ASliderOneUlpOffItsDefaultStillReadsAsDefault() {
        const float mid = 0.75f;

        Assert.AreNotEqual(mid, NextUlp(mid, 1), "The probe must be a different float, or this proves nothing.");
        Assert.IsTrue(SettingValueMath.NearlyEqual(NextUlp(mid, 1), mid));
        Assert.IsTrue(SettingValueMath.NearlyEqual(NextUlp(mid, -1), mid));
    }

    [TestMethod]
    public void ATickSettingOneUlpOffItsDefaultStillReadsAsDefault() {
        const float oneDay = 60000f;

        Assert.AreNotEqual(oneDay, NextUlp(oneDay, 1));
        Assert.IsTrue(SettingValueMath.NearlyEqual(NextUlp(oneDay, 1), oneDay));
        Assert.IsTrue(SettingValueMath.NearlyEqual(NextUlp(oneDay, -1), oneDay));
    }

    [TestMethod]
    public void NearlyEqualStillSeparatesGenuinelyDifferentSettings() {
        Assert.IsFalse(SettingValueMath.NearlyEqual(0.25f, 0.5f));
        Assert.IsFalse(SettingValueMath.NearlyEqual(60000f, 60001f));
        Assert.IsFalse(SettingValueMath.NearlyEqual(0f, 1f));
    }

    [TestMethod]
    public void BothSettingControlsCompareThroughTheTolerance() {
        foreach (string file in new[] { "SliderControl.cs", "TicksControl.cs" }) {
            string source = Core("Core", "Settings", "Model", file);

            Assert.IsFalse(
                Regex.IsMatch(source, @"Value\s*(==|!=)\s*Default") ||
                Regex.IsMatch(source, @"Value\s*\.\s*Equals\s*\(\s*Default\s*\)"),
                $"{file}: a dragged slider lands an ulp off its default and the reset button stays lit."
            );
            Assert.IsTrue(
                Regex.IsMatch(source, @"NearlyEqual\(\s*Value\s*,\s*Default\s*\)"),
                $"{file}: the default comparison no longer routes through SettingValueMath.NearlyEqual."
            );
        }
    }

    [TestMethod]
    public void AVialIsRefusedOnceThePawnIsAtFullPower() {
        string source = Core("System", "Scadrial", "FloatMenuOptionProvider", "AllomanticVialMenuProvider.cs");

        Assert.IsFalse(
            Regex.IsMatch(source, @"power\s*\.\s*Equals\s*\(\s*max\s*\)") ||
            Regex.IsMatch(source, @"power\s*==\s*max"),
            "Stat values carry factors and offsets, so a capped pawn rarely equals maxValue exactly " +
            "and was offered a lerasium vial that did nothing."
        );
        Assert.IsTrue(
            Regex.IsMatch(source, @"power\s*>=\s*max"),
            "The full-power check no longer compares power against max with a threshold at all."
        );
    }

    [TestMethod]
    public void TheCompoundedDialIsIdleAnywhereInsideItsDeadBand() {
        string source = Core("System", "Scadrial", "Gene", "Feruchemist.cs");

        Assert.IsFalse(
            Regex.IsMatch(source, @"compoundedTargetValue\s*(!=|==)\s*IdleTarget") ||
            Regex.IsMatch(source, @"compoundedTargetValue\s*\.\s*Equals\s*\(\s*IdleTarget\s*\)"),
            "A dial nudged half a point off rest produced severity 0 but still reported compounding, " +
            "which blocked the Reset that frees a feruchemist who can no longer store."
        );
        Assert.IsTrue(
            Regex.IsMatch(
                source,
                @"Mathf\.Abs\(\s*compoundedTargetValue\s*-\s*IdleTarget\s*\)\s*>=\s*DeadBand"
            ),
            "isCompounding must ask whether the dial left the dead band, not whether it moved at all."
        );
    }

    [TestMethod]
    public void KolossScalingBranchesOnAThresholdNotAnExactOne() {
        string source = Core("System", "Scadrial", "Patch", "Rendering", "KolossScalePatches.cs");

        Assert.IsFalse(
            Regex.IsMatch(source, @"grown\s*(!=|==)\s*1f") ||
            Regex.IsMatch(source, @"grown\s*\.\s*Equals\s*\(\s*1f\s*\)"),
            "A koloss whose bulk lands a hair off 1f skipped the scale patch entirely."
        );
        Assert.IsTrue(Regex.IsMatch(source, @"grown\s*>\s*1f"), "The draw patch no longer thresholds on grown.");
        Assert.IsTrue(Regex.IsMatch(source, @"grown\s*<=\s*1f"), "The hauling patch no longer thresholds on grown.");
    }
}
