using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cosmere.System.Scadrial.Grid;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Roof a vent and the ash has nowhere to go, so it goes into the room. The whole point is
///     that room size sets the pace: a closet buries in days, a hall takes a season.
/// </summary>
[TestClass]
public class AshRoomFillTests {
    /// <summary>The shipped vent's mouth rate, read out of the def rather than restated here.</summary>
    private static float ShippedMillimetresPerDay {
        get {
            string path = Path.Combine(RepoRoot, "CosmereScadrial", "Defs", "Things", "Building", "AshVent.xml");
            Assert.IsTrue(File.Exists(path), $"AshVent.xml is missing at {path}.");

            XElement? comp = XDocument.Load(path).Descendants("li")
                .FirstOrDefault(li => li.Attribute("Class")?.Value.EndsWith("CompProperties_AshVent") == true);

            Assert.IsNotNull(comp, "AshVent.xml no longer carries a CompProperties_AshVent.");

            string? rate = comp.Element("millimetresPerDay")?.Value;
            Assert.IsNotNull(rate, "the vent declares no millimetresPerDay, so it inherits the C# default.");

            return float.Parse(rate, CultureInfo.InvariantCulture);
        }
    }

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

    /// <summary>Open ground is where the plume belongs. Nothing about size may override that.</summary>
    [TestMethod]
    public void OpenSkyNeverHoldsThePlume() {
        foreach (int cells in new[] { 1, 12, 30, 441, 5000 }) {
            Assert.IsFalse(
                AshRoomFill.HoldsThePlume(cells, true), $"a {cells}-cell outdoor room claimed to hold the plume."
            );
        }
    }

    /// <summary>
    ///     The cap is the guard against a misread room turning a bounded local write into a map
    ///     scan, so it sits exactly at the write the open plume already pays for.
    /// </summary>
    [TestMethod]
    public void ARoomWiderThanThePlumeIsACavernAndIsLeftAlone() {
        Assert.IsTrue(AshRoomFill.HoldsThePlume(AshRoomFill.MaxCells, false), "the cap itself was refused.");
        Assert.IsFalse(AshRoomFill.HoldsThePlume(AshRoomFill.MaxCells + 1, false), "one cell past the cap filled.");
        Assert.IsFalse(AshRoomFill.HoldsThePlume(62500, false), "a whole map's worth of cells filled.");
    }

    [TestMethod]
    public void TheCapIsTheWriteTheOpenPlumeAlreadyPays() {
        int disc = 0;
        for (int x = -AshPlume.RadiusCells; x <= AshPlume.RadiusCells; x++) {
            for (int z = -AshPlume.RadiusCells; z <= AshPlume.RadiusCells; z++) {
                if (x * x + z * z <= AshPlume.RadiusCells * AshPlume.RadiusCells) disc++;
            }
        }

        Assert.AreEqual(disc, AshPlume.CellCount, "the plume's own cell count drifted from its disc.");
        Assert.AreEqual(AshPlume.CellCount, AshRoomFill.MaxCells, "a roofed vent writes more cells than an open one.");
    }

    /// <summary>A room with no cells is a dereferenced one. It takes nothing and divides by nothing.</summary>
    [TestMethod]
    public void ARoomWithNoCellsTakesNothing() {
        Assert.IsFalse(AshRoomFill.HoldsThePlume(0, false));
        Assert.IsFalse(AshRoomFill.HoldsThePlume(-1, false));
        Assert.AreEqual(0f, AshRoomFill.PerCellMm(140f, 0), 0.0001f);
        Assert.AreEqual(0f, AshRoomFill.PerCellMm(140f, -1), 0.0001f);
    }

    /// <summary>
    ///     The redirect moves the plume, it does not invent a second one. Every millimetre-cell the
    ///     vent would have spread across open ground lands inside the room instead.
    /// </summary>
    [TestMethod]
    public void TheWholePlumesMassEndsUpInTheRoom() {
        foreach (int cells in new[] { 1, 4, 30, 137, AshRoomFill.MaxCells }) {
            float landed = AshRoomFill.PerCellMm(140f, cells) * cells;
            float expected = 140f * AshPlume.TotalWeight;

            Assert.AreEqual(expected, landed, expected * 0.001f, $"a {cells}-cell room lost or gained mass.");
        }
    }

    [TestMethod]
    public void TheBudgetIsTheMassTheOpenPlumeWouldHaveSpread() {
        float sum = 0f;
        for (int x = -AshPlume.RadiusCells; x <= AshPlume.RadiusCells; x++) {
            for (int z = -AshPlume.RadiusCells; z <= AshPlume.RadiusCells; z++) {
                sum += AshPlume.Weight(x, z, 1f, 0f, 0f);
            }
        }

        Assert.AreEqual(sum, AshPlume.TotalWeight, 0.001f, "the cached total drifted from the weights.");
    }

    [TestMethod]
    public void ASmallerRoomFillsFaster() {
        float previous = float.MaxValue;
        for (int cells = 1; cells <= AshRoomFill.MaxCells; cells++) {
            float share = AshRoomFill.PerCellMm(140f, cells);
            Assert.IsTrue(share < previous, $"{cells} cells took {share}mm, not less than {previous}mm.");
            previous = share;
        }
    }

    /// <summary>
    ///     The shipped rate through the code that consumes it, not a number restated in a comment.
    ///     A closet has to bury what is in it inside a quadrum or roofing a vent costs the player
    ///     nothing they will ever connect to the roof.
    /// </summary>
    [TestMethod]
    public void TheShippedVentBuriesASmallRoomWithinAQuadrum() {
        float perDay = AshRoomFill.PerCellMm(ShippedMillimetresPerDay, 30);
        float days = AshDepthMath.BuriedMm / perDay;

        Assert.IsTrue(days is > 3f and < 15f, $"a 30-cell room buries in {days:0.0} days, outside the 3-15 band.");
    }

    /// <summary>
    ///     And the far end of the same dial. A room at the cap still fills, but slowly enough that
    ///     the cap never has to be a hard cut-off the player can feel.
    /// </summary>
    [TestMethod]
    public void TheShippedVentTakesSeasonsOverARoomAtTheCap() {
        float perDay = AshRoomFill.PerCellMm(ShippedMillimetresPerDay, AshRoomFill.MaxCells);
        float days = AshDepthMath.BuriedMm / perDay;

        Assert.IsTrue(days > 60f, $"a room at the cap buries in {days:0.0} days, fast enough to feel like a cliff.");
    }

    /// <summary>
    ///     A cycle's share has to survive the bank. At the cap it is a hundredth of a grid unit, so
    ///     depositing it straight would floor every cycle to zero and the room would never fill.
    /// </summary>
    [TestMethod]
    public void ACyclesShareIsBankedRatherThanLost() {
        const float cycleDays = 64f / 60000f;
        const int cyclesPerDay = 937;
        const int days = 30;

        float share = AshRoomFill.PerCellMm(ShippedMillimetresPerDay * cycleDays, AshRoomFill.MaxCells);
        Assert.IsTrue(share < AshGrid.UnitMm, $"a cycle's share is {share}mm, so the bank is not what carries it.");

        float remainder = 0f;
        int deposited = 0;
        for (int i = 0; i < cyclesPerDay * days; i++) {
            deposited += AshPlume.Bank(ref remainder, share, AshGrid.UnitMm);
        }

        float added = share * cyclesPerDay * days;
        Assert.IsTrue(
            deposited > added - AshGrid.UnitMm && deposited <= added,
            $"{days} days of cycles added {added:0.0}mm and deposited {deposited}mm."
        );
    }

    /// <summary>
    ///     AddDepthMm's contract: callers owe the buried set. The room fill writes far faster than
    ///     the map sweep, so it owes its own refresh rather than the 64-tick lag. Five bugs deep,
    ///     this reads the source because nothing else here can see a MapComponent.
    /// </summary>
    [TestMethod]
    public void TheRoomFillPaysWhatItOwesTheBuriedSet() {
        string body = MethodContaining(VentSource, "AshRoomFill.PerCellMm");

        StringAssert.Contains(body, "AddDepthMm", "the room fill no longer writes depth.");
        StringAssert.Contains(body, "buried.Set(", "the room fill writes depth and leaves the buried set stale.");
    }

    /// <summary>
    ///     The trap the sealed vent has to dodge. The feather allows nothing to stand on the mouth,
    ///     so a room made only of the mouth would have its whole fill swept away every cycle - a
    ///     cap built out of eight walls, which is the thing the design refused to sell.
    /// </summary>
    [TestMethod]
    public void ARoomTheSizeOfTheMouthIsStillAFillableRoom() {
        Assert.AreEqual(0, AshPlume.AllowedDepthMm(AshGrid.MaxDepthMm, 0f, 3f), "the mouth kept some ash.");
        Assert.IsTrue(AshRoomFill.HoldsThePlume(4, false), "a 2x2 box refused to hold the plume.");
    }

    /// <summary>
    ///     And the guard on it. Mouth clearing has to sit inside the branch that drifts, not run
    ///     after both, or the sweep cancels every room small enough to sit inside the feather.
    /// </summary>
    [TestMethod]
    public void ASealedVentStopsSweepingItsOwnMouth() {
        string body = MethodContaining(VentSource, "SealedRoom(");

        int drift = Indentation(body, "= Drift(");
        int clear = Indentation(body, "ClearOwnMouth(");

        Assert.AreEqual(
            drift,
            clear,
            "mouth clearing left the drift branch, so a sealed vent sweeps away everything it deposits."
        );
    }

    private static string VentSource {
        get {
            string path = Path.Combine(
                RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Comp", "Thing", "CompAshVent.cs"
            );
            Assert.IsTrue(File.Exists(path), $"CompAshVent.cs is missing at {path}.");

            return File.ReadAllText(path);
        }
    }

    private static int Indentation(string body, string fragment) {
        string? line = body.Split('\n').FirstOrDefault(l => l.Contains(fragment));
        Assert.IsNotNull(line, $"no line of ContributeToGrid mentions {fragment}.");

        return line.Length - line.TrimStart().Length;
    }

    /// <summary>The method holding a fingerprint, cut at the four-space brace that closes it.</summary>
    private static string MethodContaining(string source, string fingerprint) {
        string[] lines = source.Split('\n');
        int at = Array.FindIndex(lines, line => line.Contains(fingerprint));
        Assert.IsTrue(at >= 0, $"no line in CompAshVent.cs mentions {fingerprint}.");

        int start = at;
        while (start > 0 && !lines[start].StartsWith("    private ") && !lines[start].StartsWith("    public ")) {
            start--;
        }

        int end = at;
        while (end < lines.Length - 1 && lines[end].TrimEnd('\r') != "    }") {
            end++;
        }

        return string.Join("\n", lines, start, end - start + 1);
    }
}
