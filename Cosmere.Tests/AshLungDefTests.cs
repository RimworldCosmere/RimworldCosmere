using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cosmere.System.Scadrial.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     AshLung's stage thresholds only mean something next to the rate that drives them. Retune
///     AshLungMath without re-spacing the stages and the progression quietly stops being one.
/// </summary>
[TestClass]
public class AshLungDefTests {
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

    private static XElement AshLung {
        get {
            string path = Path.Combine(RepoRoot, "CosmereScadrial", "Defs", "Hediffs", "AshLung.xml");
            Assert.IsTrue(File.Exists(path), $"AshLung.xml is missing at {path}.");

            XElement? def = XDocument.Load(path).Root?.Elements("HediffDef")
                .FirstOrDefault(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Hediff_AshLung");

            Assert.IsNotNull(def, "AshLung.xml no longer declares Cosmere_Scadrial_Hediff_AshLung.");
            return def;
        }
    }

    private static List<XElement> Stages {
        get {
            XElement? stages = AshLung.Element("stages");
            Assert.IsNotNull(stages, "AshLung declares no stages, so it has no progression at all.");
            return stages.Elements("li").ToList();
        }
    }

    private static float Float(XElement? element, float fallback) =>
        element == null ? fallback : float.Parse(element.Value, CultureInfo.InvariantCulture);

    [TestMethod]
    public void StageThresholdsSitWhereTheGainRatePutsThem() {
        List<XElement> stages = Stages;
        Assert.IsTrue(stages.Count >= 3, "The progression needs at least three stages to read as one.");

        float perHour = AshLungMath.SeverityDeltaPerHour(1f, 0f);
        Assert.IsTrue(perHour > 0f, "Full unmasked exposure must gain severity, or none of this applies.");

        float[] thresholds = stages.Select(s => Float(s.Element("minSeverity"), 0f)).ToArray();
        for (int i = 1; i < thresholds.Length; i++) {
            Assert.IsTrue(
                thresholds[i] > thresholds[i - 1],
                $"Stage {i} opens at {thresholds[i]}, no later than stage {i - 1} at {thresholds[i - 1]}.");
        }

        float firstVisibleHours = thresholds[1] / perHour;
        Assert.IsTrue(
            firstVisibleHours is >= 2f and <= 12f,
            $"The first visible stage lands after {firstVisibleHours:0.#}h of unmasked exposure. It should read as " +
            "a shift worked beside a vent - not one step through the gas, and not a whole day of it.");

        float worstHours = thresholds[^1] / perHour;
        Assert.IsTrue(
            worstHours >= 48f,
            $"The worst stage lands after {worstHours:0.#}h. Reaching it should take deliberate neglect.");

        float lethalHours = Float(AshLung.Element("lethalSeverity"), float.MaxValue) / perHour;
        Assert.IsTrue(
            lethalHours >= 72f,
            $"Ash lung kills after {lethalHours:0.#}h of unbroken exposure, faster than a player can be expected " +
            "to read the escalation and act on it.");
    }

    /// <summary>
    ///     Breathing is the whole point. A stage labelled ash lung that leaves the lungs alone is
    ///     a name with nothing behind it, and each stage has to bite harder than the last.
    /// </summary>
    [TestMethod]
    public void EveryVisibleStageCutsBreathingHarderThanTheLast() {
        List<string> offenders = [];
        float previous = 0f;

        foreach (XElement stage in Stages) {
            if (stage.Element("becomeVisible")?.Value.Trim() == "false") continue;

            float offset = stage.Element("capMods")?.Elements("li")
                .Where(mod => mod.Element("capacity")?.Value.Trim() == "Breathing")
                .Select(mod => Float(mod.Element("offset"), 0f))
                .FirstOrDefault() ?? 0f;

            string label = stage.Element("label")?.Value ?? "(unlabelled)";
            if (offset >= 0f) offenders.Add($"'{label}' does not reduce Breathing");
            else if (previous < 0f && offset >= previous) offenders.Add($"'{label}' is no worse than the stage before");

            previous = offset;
        }

        Assert.AreEqual(0, offenders.Count, string.Join("; ", offenders));
    }

    /// <summary>
    ///     HediffStage.regeneration exists on the type but Anomaly gates it, so in a Core plus
    ///     Biotech plus Ideology loadout it does nothing and says nothing about it.
    /// </summary>
    [TestMethod]
    public void NoCosmereHediffStageUsesRegeneration() {
        List<string> offenders = [];

        foreach (string mod in new[] { "CosmereCore", "CosmereScadrial", "CosmereRoshar" }) {
            string dir = Path.Combine(RepoRoot, mod, "Defs");
            if (!Directory.Exists(dir)) continue;

            foreach (string path in Directory.GetFiles(dir, "*.xml", SearchOption.AllDirectories)) {
                XElement? root = XDocument.Load(path).Root;
                if (root == null) continue;

                foreach (XElement def in root.Descendants("HediffDef")) {
                    if (!def.Descendants("regeneration").Any()) continue;

                    offenders.Add($"{def.Element("defName")?.Value ?? "(abstract)"} in {Path.GetFileName(path)}");
                }
            }
        }

        string detail = "These hediff stages set regeneration, which Anomaly gates off. Use InjuryHealingFactor " +
            "instead: " + string.Join(", ", offenders);
        Assert.AreEqual(0, offenders.Count, detail);
    }
}
