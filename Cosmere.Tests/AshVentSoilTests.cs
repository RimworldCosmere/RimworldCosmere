using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Cosmere.System.Scadrial.Grid;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Volcanic ground is fertile, so the patch a vent keeps swept is worth farming. The whole
///     feature rests on that patch never being deep enough for the ash to take the terrain back.
/// </summary>
[TestClass]
public class AshVentSoilTests {
    /// <summary>Vanilla SoilRich. Vent soil has to beat it or there is no reason to farm a vent.</summary>
    private const float VanillaRichSoilFertility = 1.4f;

    /// <summary>Core's own terrain folder. A path under it resolves without art of ours.</summary>
    private const string VanillaTexturePrefix = "Terrain/Surfaces/";

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

    private static string TerrainDefsDir => Path.Combine(RepoRoot, "CosmereScadrial", "Defs", "Terrain");

    private static XElement VentSoil {
        get {
            string path = Path.Combine(TerrainDefsDir, "VentSoil.xml");
            Assert.IsTrue(File.Exists(path), $"VentSoil.xml is missing at {path}.");

            XElement? def = XDocument.Load(path).Root?.Elements("TerrainDef")
                .FirstOrDefault(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Terrain_VentSoil");

            Assert.IsNotNull(def, "VentSoil.xml no longer declares Cosmere_Scadrial_Terrain_VentSoil.");

            return def!;
        }
    }

    [TestMethod]
    public void VentSoilOutgrowsVanillaRichSoil() {
        string? fertility = VentSoil.Element("fertility")?.Value;

        Assert.IsNotNull(fertility, "Vent soil declares no fertility, so it inherits 1.0 and beats nothing.");
        Assert.IsTrue(
            float.Parse(fertility!, CultureInfo.InvariantCulture) > VanillaRichSoilFertility,
            $"vent soil sits at {fertility}, at or under vanilla rich soil's {VanillaRichSoilFertility}."
        );
    }

    /// <summary>Without GrowSoil the player cannot sow it, whatever the fertility says.</summary>
    [TestMethod]
    public void VentSoilCanBeSown() {
        List<string> affordances = VentSoil.Element("affordances")?.Elements("li").Select(li => li.Value).ToList()
                                   ?? [];

        CollectionAssert.Contains(affordances, "GrowSoil", "vent soil grants no GrowSoil affordance.");
    }

    /// <summary>
    ///     A texture path that resolves to nothing is a red error at load, which this feature has
    ///     already shipped once. Ours need a file; Core's own folder is taken on trust.
    /// </summary>
    [TestMethod]
    public void EveryScadrialTerrainTextureExists() {
        foreach (string file in Directory.GetFiles(TerrainDefsDir, "*.xml")) {
            foreach (XElement def in XDocument.Load(file).Root?.Elements("TerrainDef") ?? []) {
                string defName = def.Element("defName")?.Value ?? Path.GetFileName(file);
                string? texture = def.Element("texturePath")?.Value;

                Assert.IsFalse(string.IsNullOrWhiteSpace(texture), $"{defName} declares no texturePath.");
                if (texture!.StartsWith(VanillaTexturePrefix, StringComparison.Ordinal)) continue;

                string png = Path.Combine(RepoRoot, "CosmereScadrial", "Assets", "Textures", texture + ".png");
                Assert.IsTrue(File.Exists(png), $"{defName} points at {texture}, and no PNG exists at {png}.");
            }
        }
    }

    /// <summary>
    ///     GetNamed throws when the def is missing, so a defName the C# looks up but no XML
    ///     declares takes the map component down rather than logging and carrying on.
    /// </summary>
    [TestMethod]
    public void EveryTerrainDefNameTheCodeLooksUpExists() {
        HashSet<string> declared = [];
        foreach (string file in Directory.GetFiles(TerrainDefsDir, "*.xml")) {
            foreach (XElement def in XDocument.Load(file).Root?.Elements("TerrainDef") ?? []) {
                string? defName = def.Element("defName")?.Value;
                if (defName != null) declared.Add(defName);
            }
        }

        string scadrial = Path.Combine(RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial");
        Regex pattern = new Regex("\"(Cosmere_Scadrial_Terrain_[A-Za-z0-9_]+)\"");
        int found = 0;

        foreach (string source in Directory.GetFiles(scadrial, "*.cs", SearchOption.AllDirectories)) {
            foreach (Match match in pattern.Matches(File.ReadAllText(source))) {
                found++;
                string defName = match.Groups[1].Value;
                Assert.IsTrue(
                    declared.Contains(defName),
                    $"{Path.GetFileName(source)} looks up {defName}, which no TerrainDef in Defs/Terrain declares."
                );
            }
        }

        Assert.IsTrue(found >= 2, $"expected the ash and vent soil lookups, found {found}.");
    }

    /// <summary>
    ///     The guard the whole placement rests on. Anywhere the vent lays soil, the feather's own
    ///     ceiling has to stay under the depth at which a cell turns into ash terrain.
    /// </summary>
    [TestMethod]
    public void SoilIsOnlyLaidWhereAshCanNeverTakeTheTerrain() {
        for (float radius = 0f; radius <= 12f; radius += 0.5f) {
            for (float distance = 0f; distance <= 16f; distance += 0.05f) {
                if (!AshPlume.StaysBelowTheSwap(distance, radius)) continue;

                int ceiling = AshPlume.AllowedDepthMm(AshGrid.MaxDepthMm, distance, radius);
                Assert.IsTrue(
                    ceiling < AshDepthMath.TerrainSwapMm,
                    $"radius {radius}, {distance} cells out: the feather allows {ceiling}mm, at or over the "
                    + $"{AshDepthMath.TerrainSwapMm}mm swap point, so soil laid there would go under ash."
                );
            }
        }
    }

    /// <summary>The mouth is swept to bare ground at every radius, so it always takes soil.</summary>
    [TestMethod]
    public void TheMouthItselfAlwaysTakesSoil() {
        foreach (float radius in new[] { 0f, 1f, 3f, 12f }) {
            Assert.IsTrue(AshPlume.StaysBelowTheSwap(0f, radius), $"radius {radius} refused its own mouth.");
        }
    }

    /// <summary>Past the feather the vent thins nothing, so anything laid there is on borrowed time.</summary>
    [TestMethod]
    public void NothingIsLaidPastTheFeather() {
        Assert.IsFalse(AshPlume.StaysBelowTheSwap(3f, 3f), "the outer edge is the first untouched ring.");
        Assert.IsFalse(AshPlume.StaysBelowTheSwap(9f, 3f));
        Assert.IsFalse(AshPlume.StaysBelowTheSwap(1f, 0f), "a zero radius reached a cell off the mouth.");
    }

    /// <summary>
    ///     At the shipped radius 3 the patch has to be worth walking to. A rule that only ever
    ///     returned the 2x2 mouth would pass every other test here and hand the player nothing.
    /// </summary>
    [TestMethod]
    public void TheShippedRadiusGivesAPatchWorthFarming() {
        int cells = 0;
        for (int x = -4; x <= 5; x++) {
            for (int z = -4; z <= 5; z++) {
                int dx = x < 0 ? -x : x > 1 ? x - 1 : 0;
                int dz = z < 0 ? -z : z > 1 ? z - 1 : 0;
                if (AshPlume.StaysBelowTheSwap((float)Math.Sqrt(dx * dx + dz * dz), 3f)) cells++;
            }
        }

        // A 2x2 mouth grown to distance 2.236: 32 cells, the four diagonal corners at 2.828 cut.
        Assert.AreEqual(32, cells, "the vent soil patch changed size.");
    }
}
