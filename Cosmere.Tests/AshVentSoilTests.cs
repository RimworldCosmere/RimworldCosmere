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
///     Volcanic ground is fertile, so the patch a vent keeps swept is worth farming. Inside the
///     clearing the ash can never take that patch back; past it, AshTerrainMemory hands it back.
/// </summary>
[TestClass]
public class AshVentSoilTests {
    /// <summary>Vanilla SoilRich. Vent soil has to beat it or there is no reason to farm a vent.</summary>
    private const float VanillaRichSoilFertility = 1.4f;

    /// <summary>Core's own terrain folder. A path under it resolves without art of ours.</summary>
    private const string VanillaTexturePrefix = "Terrain/Surfaces/";

    /// <summary>GenDate.TicksPerDay, which the test project deliberately cannot load.</summary>
    private const float TicksPerDay = 60000f;

    /// <summary>AshDepthTracker.Stripes. A vent contributes once a cycle, on stripe 0.</summary>
    private const float CycleTicks = 64f;

    private const float CycleDays = CycleTicks / TicksPerDay;

    /// <summary>Days in a quadrum. What "noticed across a season" is measured against.</summary>
    private const float QuadrumDays = 15f;

    /// <summary>The vent's own clearing, from CompProperties_AshVent.clearRadius.</summary>
    private const float ClearRadius = 3f;

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

    [TestMethod]
    public void TheDefaultReachIsEightCells() {
        Assert.AreEqual(8f, AshVentSoilSpread.DefaultReachCells);
    }

    [TestMethod]
    public void TheSettingRangeHoldsTheDefault() {
        Assert.IsTrue(
            AshVentSoilSpread.MinReachCells <= AshVentSoilSpread.DefaultReachCells
            && AshVentSoilSpread.MaxReachCells >= AshVentSoilSpread.DefaultReachCells,
            $"the default {AshVentSoilSpread.DefaultReachCells} sits outside "
            + $"[{AshVentSoilSpread.MinReachCells}, {AshVentSoilSpread.MaxReachCells}], so the slider cannot show it."
        );
    }

    /// <summary>The vent holds its own clearing fertile from the day it spawns, then creeps out.</summary>
    [TestMethod]
    public void AFreshVentStartsAtItsOwnClearing() {
        Assert.AreEqual(ClearRadius, AshVentSoilSpread.Advance(0f, ClearRadius, 8f, 0f), 0.0001f);
    }

    [TestMethod]
    public void TheFrontStopsAtTheConfiguredReach() {
        Assert.AreEqual(8f, AshVentSoilSpread.Advance(ClearRadius, ClearRadius, 8f, 10000f), 0.0001f);
    }

    /// <summary>Turning the setting down pulls the front in rather than stranding it past the cap.</summary>
    [TestMethod]
    public void AReachUnderTheClearingPullsTheFrontIn() {
        Assert.AreEqual(2f, AshVentSoilSpread.Advance(8f, ClearRadius, 2f, 0f), 0.0001f);
    }

    /// <summary>Clearing 3 out to the default 8 is five cells at eight days each: forty days.</summary>
    [TestMethod]
    public void TheFrontTakesFortyDaysToReachTheDefault() {
        float front = AshVentSoilSpread.Advance(0f, ClearRadius, 8f, 0f);
        float short_ = (5f * AshVentSoilSpread.DaysPerCell) - 1f;

        front = AshVentSoilSpread.Advance(front, ClearRadius, 8f, short_);
        Assert.IsTrue(front < 8f, $"the front reached the default in {short_} days, at {front}.");

        front = AshVentSoilSpread.Advance(front, ClearRadius, 8f, 1.1f);
        Assert.AreEqual(8f, front, 0.0001f, "five cells of dwell did not carry the front to the default 8.");
    }

    /// <summary>
    ///     The rate claim, counted in cells. A day has to read as nothing and a quadrum as a
    ///     wider band, or the spread is either invisible or a jump.
    /// </summary>
    [TestMethod]
    public void ADaysCreepIsInvisibleAndAQuadrumsIsNot() {
        float front = AshVentSoilSpread.DefaultReachCells;
        int total = CellsInside(front);

        int inADay = total - CellsInside(front - 1f / AshVentSoilSpread.DaysPerCell);
        int inAQuadrum = total - CellsInside(front - QuadrumDays / AshVentSoilSpread.DaysPerCell);

        Assert.IsTrue(inADay <= 10, $"a day adds {inADay} cells of {total}, which a player reads as a jump.");
        Assert.IsTrue(inAQuadrum >= 50, $"a quadrum adds only {inAQuadrum} cells of {total}, which reads as nothing.");
    }

    /// <summary>
    ///     The banked front is the whole record. Thirty days in one step and thirty days a cycle
    ///     at a time have to land in the same place, or a reload would shift the ground.
    /// </summary>
    [TestMethod]
    public void CycleByCycleMatchesOneLongStep() {
        float stepped = AshVentSoilSpread.Advance(ClearRadius, ClearRadius, 8f, 30f);

        float banked = ClearRadius;
        int cycles = (int)(30f / CycleDays);
        for (int i = 0; i < cycles; i++) {
            banked = AshVentSoilSpread.Advance(banked, ClearRadius, 8f, CycleDays);
        }

        // 28125 float adds, so the two drift apart by rounding rather than by rule.
        Assert.AreEqual(stepped, banked, 0.02f, "the front moved differently when it was advanced a cycle at a time.");
    }

    /// <summary>
    ///     One cycle is 0.0011 days, so the step is 0.00009 cells. Banked on a float already out
    ///     at 7.5 it still has to move, or the front stalls short of the setting forever.
    /// </summary>
    [TestMethod]
    public void OneCyclesStepSurvivesFloatPrecision() {
        const float front = 7.5f;

        Assert.IsTrue(
            AshVentSoilSpread.Advance(front, ClearRadius, 16f, CycleDays) > front,
            $"a cycle's step vanished into the float at {front}."
        );
    }

    /// <summary>
    ///     The ration is what stops six vents firing hundreds of SetTerrain calls on one tick. It
    ///     still has to outpace the front, or the spread never reaches the setting.
    /// </summary>
    [TestMethod]
    public void TheWriteRationOutpacesTheFront() {
        float front = AshVentSoilSpread.DefaultReachCells;
        int openedPerDay = CellsInside(front) - CellsInside(front - 1f / AshVentSoilSpread.DaysPerCell);
        float writesPerDay = AshDepthMath.TerrainChangesPerSweep * (TicksPerDay / CycleTicks);

        Assert.IsTrue(
            openedPerDay < writesPerDay,
            $"the front opens {openedPerDay} cells a day against {writesPerDay} rationed writes."
        );
    }

    /// <summary>At the default the patch is worth the walk, and a change to its shape is deliberate.</summary>
    [TestMethod]
    public void TheDefaultReachGivesAPatchWorthFarming() {
        Assert.AreEqual(
            232, CellsInside(AshVentSoilSpread.DefaultReachCells), "the vent soil patch changed size."
        );
    }

    /// <summary>
    ///     The front is banked state. Soil in the terrain grid says a cell was reached, never
    ///     whether its neighbours were, and this feature has shipped unsaved state twice already.
    /// </summary>
    [TestMethod]
    public void TheSpreadFrontIsScribed() {
        string comp = Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Comp", "Thing", "CompAshVent.cs"
        );

        Assert.IsTrue(File.Exists(comp), $"CompAshVent.cs is missing at {comp}.");
        StringAssert.Contains(
            File.ReadAllText(comp),
            "Scribe_Values.Look(ref soilRadius",
            "the spread front is not saved, so every reload would walk it back out from the clearing."
        );
    }

    /// <summary>
    ///     Cells a front of this radius covers around the 2x2 mouth, by the same out-of-rect
    ///     distance the comp walks.
    /// </summary>
    private static int CellsInside(float radius) {
        int reach = (int)radius + 2;
        int cells = 0;

        for (int x = -reach; x <= 1 + reach; x++) {
            for (int z = -reach; z <= 1 + reach; z++) {
                int dx = x < 0 ? -x : x > 1 ? x - 1 : 0;
                int dz = z < 0 ? -z : z > 1 ? z - 1 : 0;
                if (AshVentSoilSpread.Reaches(dx * dx + dz * dz, radius)) cells++;
            }
        }

        return cells;
    }
}
