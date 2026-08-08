using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The burial wash has to land on top of dropped items and stop short of pawns. A SectionLayer
///     and a Material are both Verse-bound, so these read the layer source rather than running it.
/// </summary>
[TestClass]
public class AshBurialRenderTests {
    /// <summary>Map/Cutout and Map/Transparent both declare Queue = Transparent-100.</summary>
    private const int PrintedThingsQueue = 2900;

    /// <summary>MapMaterialRenderQueues.Blueprint.</summary>
    private const int BlueprintQueue = 2950;

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

    private static string LayerSource {
        get {
            string path = Path.Combine(
                RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Render", "SectionLayer_AshBurial.cs"
            );

            Assert.IsTrue(File.Exists(path), $"Expected the burial layer at {path}");
            return File.ReadAllText(path);
        }
    }

    [TestMethod]
    public void TheWashMaterialCarriesAnExplicitRenderQueue() {
        Assert.IsTrue(
            Regex.IsMatch(LayerSource, @"MatFrom\([^)]*ShaderDatabase\.Transparent\s*,\s*\w+\s*\)"),
            "The wash material needs a third MatFrom argument. Without it the material keeps the shader " +
            $"default of {PrintedThingsQueue}, ties with the printed things mesh, and whether it composites " +
            "over items comes down to the order GenTypes.AllSubclassesNonAbstract happened to return."
        );
    }

    [TestMethod]
    public void TheWashQueueSitsAbovePrintedThingsAndBelowBlueprints() {
        Match match = Regex.Match(LayerSource, @"WashRenderQueue\s*=\s*(\d+)\s*;");
        Assert.IsTrue(match.Success, "Expected a WashRenderQueue constant on the burial layer.");

        int queue = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        Assert.IsTrue(
            queue > PrintedThingsQueue,
            $"{queue} does not beat the printed things at {PrintedThingsQueue}, so items stay on top of the ash."
        );
        Assert.IsTrue(
            queue < BlueprintQueue,
            $"{queue} would bury blueprints at {BlueprintQueue}, leaving the player unable to see what they placed."
        );
    }

    [TestMethod]
    public void TheWashPrintsBelowPawns() {
        Assert.IsTrue(
            LayerSource.Contains("AltitudeLayer.ItemImportant"),
            "Altitude is the only thing keeping the wash off pawns now that its render queue draws after them. " +
            "ItemImportant is above AltitudeLayer.Item, which every item def defaults to, and below " +
            "AltitudeLayer.Pawn, so the depth test still discards the wash where a colonist stands."
        );
    }
}
