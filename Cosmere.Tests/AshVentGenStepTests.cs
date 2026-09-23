using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Ash vents were being sited before the gen steps that lay floors, so a later ruin or shrine
///     painted tile over the top of one. The siting lives in XML, and a misspelled validator class
///     is dropped at load without an exception, so both are pinned here.
/// </summary>
[TestClass]
public class AshVentGenStepTests {
    /// <summary>SteamGeysers' slot. Ruins, shrines and scenario structures all run before it.</summary>
    private const string GeyserOrder = "950";

    private const string VentGenStepName = "Cosmere_Scadrial_GenStep_AshVents";

    /// <summary>Every scatterer validator Verse defines. Note the one "r" on AvoidUsedRects.</summary>
    private static readonly HashSet<string> KnownValidators = [
        "ScattererValidator_Buildable",
        "ScattererValidator_NoNonNaturalEdifices",
        "ScattererValidator_AvoidThingsOfDef",
        "ScattererValidator_AvoidSpecialThings",
        "ScattererValidator_TerrainDef",
        "ScattererValidator_Roof",
        "ScatterValidator_AvoidUsedRects"
    ];

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

    private static XElement VentGenStep() {
        string dir = Path.Combine(RepoRoot, "CosmereScadrial", "Defs");

        foreach (string path in Directory.GetFiles(dir, "*.xml", SearchOption.AllDirectories)) {
            XElement? root = XDocument.Load(path).Root;
            if (root == null) continue;

            foreach (XElement def in root.Elements("GenStepDef")) {
                if (def.Element("defName")?.Value.Trim() == VentGenStepName) return def;
            }
        }

        Assert.Fail($"No GenStepDef named {VentGenStepName} under CosmereScadrial/Defs.");
        return null;
    }

    [TestMethod]
    public void VentsAreSitedNoEarlierThanGeysers() {
        Assert.AreEqual(
            GeyserOrder,
            VentGenStep().Element("order")?.Value.Trim(),
            "an earlier order puts vents down before ruins and scenario structures lay their floors"
        );
    }

    [TestMethod]
    public void EveryValidatorClassIsOneVerseDefines() {
        XElement genStep = VentGenStep().Element("genStep")!;
        int seen = 0;

        foreach (string list in new[] { "validators", "fallbackValidators" }) {
            foreach (XElement li in genStep.Element(list)?.Elements("li") ?? []) {
                string name = li.Attribute("Class")?.Value.Trim() ?? string.Empty;
                Assert.IsTrue(
                    KnownValidators.Contains(name),
                    $"{list} carries '{name}', which Verse does not define - the game drops it silently"
                );
                seen++;
            }
        }

        Assert.IsTrue(seen > 0, "the vent gen step should still carry validators");
    }

    [TestMethod]
    public void TheGenStepNamesTheThingItScatters() {
        XElement genStep = VentGenStep().Element("genStep")!;

        Assert.AreEqual(
            "Cosmere_Scadrial_Thing_AshVent",
            genStep.Element("thingDef")?.Value.Trim(),
            "GenStep_ScatterThings dereferences thingDef on every candidate cell"
        );
    }

    /// <summary>
    ///     The one place we deliberately part company with the geyser, so it needs a guard against
    ///     anyone restoring parity by reflex.
    /// </summary>
    [TestMethod]
    public void ClearedSpaceIsSizedForTheVentNotForAGeothermalGenerator() {
        XElement genStep = VentGenStep().Element("genStep")!;

        Assert.AreEqual(
            "12",
            genStep.Element("clearSpaceSize")?.Value.Trim(),
            "a cell count - the geyser's 30 is room for a 6x6 generator, and a 2x2 vent carries none"
        );
    }
}
