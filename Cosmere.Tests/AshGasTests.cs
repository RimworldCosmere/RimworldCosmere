using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cosmere.System.Scadrial.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The vent's reach and the rate ash builds in a lung are two different knobs, and only one of
///     them is allowed to set the pace. These keep the def from quietly growing a second one.
/// </summary>
[TestClass]
public class AshGasTests {
    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereScadrial/Defs above the test output directory.");
            return dir!.FullName;
        }
    }

    private static XElement Vent {
        get {
            string path = Path.Combine(RepoRoot, "CosmereScadrial", "Defs", "Things", "Building", "AshVent.xml");
            Assert.IsTrue(File.Exists(path), $"AshVent.xml is missing at {path}.");

            XElement? def = XDocument.Load(path).Root?.Elements("ThingDef")
                .FirstOrDefault(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Thing_AshVent");

            Assert.IsNotNull(def, "AshVent.xml no longer declares Cosmere_Scadrial_Thing_AshVent.");
            return def!;
        }
    }

    private static XElement Comp(string className) {
        XElement? comp = Vent.Element("comps")?.Elements("li")
            .FirstOrDefault(c => c.Attribute("Class")?.Value.EndsWith(className, StringComparison.Ordinal) == true);

        Assert.IsNotNull(comp, $"The ash vent carries no {className}.");
        return comp!;
    }

    private static float Float(XElement? element, float fallback) =>
        element == null ? fallback : float.Parse(element.Value, CultureInfo.InvariantCulture);

    [TestMethod]
    public void DistanceIsMeasuredFromTheFootprintNotItsCentre() {
        Assert.AreEqual(0f, AshGasFalloff.DistanceToMouth(0, 0, 0, 0, 1, 1), 0.0001f);
        Assert.AreEqual(0f, AshGasFalloff.DistanceToMouth(1, 1, 0, 0, 1, 1), 0.0001f);
        Assert.AreEqual(1f, AshGasFalloff.DistanceToMouth(2, 1, 0, 0, 1, 1), 0.0001f);
        Assert.AreEqual(1f, AshGasFalloff.DistanceToMouth(-1, 0, 0, 0, 1, 1), 0.0001f);
        Assert.AreEqual((float)Math.Sqrt(2), AshGasFalloff.DistanceToMouth(2, 2, 0, 0, 1, 1), 0.0001f);
    }

    [TestMethod]
    public void ExposureIsFullAtTheMouthGoneAtTheRadiusAndFallsAllTheWay() {
        Assert.AreEqual(1f, AshGasFalloff.Exposure(0f, 5f, 1f), 0.0001f);
        Assert.AreEqual(0f, AshGasFalloff.Exposure(5f, 5f, 1f), 0.0001f);
        Assert.AreEqual(0f, AshGasFalloff.Exposure(9f, 5f, 1f), 0.0001f);

        float previous = float.MaxValue;
        for (int step = 0; step <= 20; step++) {
            float here = AshGasFalloff.Exposure(step * 0.25f, 5f, 1f);
            Assert.IsTrue(here >= 0f && here <= 1f, $"Exposure {here} at {step * 0.25f} cells is outside 0-1.");
            Assert.IsTrue(here < previous, $"Exposure did not drop between {(step - 1) * 0.25f} and {step * 0.25f}.");
            previous = here;
        }
    }

    /// <summary>
    ///     Over-wide props are a def typo, not a licence to make one vent worse than full exposure.
    /// </summary>
    [TestMethod]
    public void AMouthStrengthAboveOneCannotPushExposurePastFullyExposed() {
        Assert.AreEqual(1f, AshGasFalloff.Exposure(0f, 5f, 9f), 0.0001f);
        Assert.IsTrue(AshGasFalloff.Exposure(1f, 5f, 9f) <= 1f);
        Assert.IsTrue(AshGasFalloff.Exposure(2f, 5f, -3f) >= 0f);
    }

    /// <summary>
    ///     The brief asks for an edge that is nearly harmless. Nearly harmless means the crossover
    ///     in AshLungMath, not a number picked to look small.
    /// </summary>
    [TestMethod]
    public void TheMouthHarmsAndTheOuterRingIsAlreadyRelief() {
        float radius = Float(Comp("CompProperties_AshGas").Element("radius"), 0f);
        Assert.IsTrue(radius >= 2f, $"A radius of {radius} leaves no room for a falloff at all.");

        float atMouth = AshGasFalloff.Exposure(0f, radius, 1f);
        Assert.IsTrue(
            AshLungMath.SeverityDeltaPerHour(atMouth, 0f) > 0f,
            "Standing unmasked on the vent mouth has to cost something."
        );

        float atEdge = AshGasFalloff.Exposure(radius - 1f, radius, 1f);
        Assert.IsTrue(
            AshLungMath.SeverityDeltaPerHour(atEdge, 0f) < 0f,
            $"A cell one in from the radius sits at exposure {atEdge:0.###}, which still harms. The outer ring " +
            "should read as thin air a pawn can work in."
        );
    }

    /// <summary>
    ///     Task 2 spaced the hediff's stages against AshLungMath at exposure 1.0 on the mouth. A
    ///     second rate on the def would stretch that timeline with nothing to catch it.
    /// </summary>
    [TestMethod]
    public void TheVentLeavesTheSeverityRateToAshLungMath() {
        XElement gas = Comp("CompProperties_AshGas");

        Assert.AreEqual(
            1f,
            Float(gas.Element("exposureAtMouth"), 1f),
            0.0001f,
            "The mouth has to sit at full exposure or AshLungDefTests is measuring a timeline nobody plays."
        );

        foreach (XElement field in gas.Elements()) {
            Assert.IsFalse(
                field.Name.LocalName.IndexOf("severity", StringComparison.OrdinalIgnoreCase) >= 0,
                $"The gas comp declares '{field.Name.LocalName}'. Severity per hour belongs to AshLungMath alone; " +
                "two rate knobs fight and the stage spacing stops meaning anything."
            );
        }
    }

    /// <summary>The heat is a vanilla comp, so the only thing worth guarding is that it is there.</summary>
    [TestMethod]
    public void TheVentPushesHeatHardEnoughToCookARoom() {
        XElement heat = Comp("CompProperties_HeatPusher");

        Assert.IsTrue(
            Float(heat.Element("heatPerSecond"), 0f) > 0f, "A heat pusher pushing nothing is an empty comp entry."
        );
        Assert.IsTrue(
            Float(heat.Element("heatPushMaxTemperature"), 0f) >= 60f,
            "Capped this low a sealed room only gets uncomfortable. The vent is meant to cook one."
        );
    }
}
