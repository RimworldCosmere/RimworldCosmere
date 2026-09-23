using System;
using System.Collections.Generic;
using System.IO;
using Cosmere.System.Scadrial.Grid;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The vent warms the ground and feeds it nutrients, so terrain climbs a chain a rung at a
///     time instead of flipping. These pin the whole chain, not one rung, and the rate it climbs at.
/// </summary>
[TestClass]
public class AshVentSoilLadderTests {
    /// <summary>GenDate.TicksPerDay, which the test project deliberately cannot load.</summary>
    private const float TicksPerDay = 60000f;

    /// <summary>AshDepthTracker.Stripes. A vent contributes once a cycle, on stripe 0.</summary>
    private const float CycleTicks = 64f;

    private const float CycleDays = CycleTicks / TicksPerDay;

    /// <summary>The vent's own clearing, from CompProperties_AshVent.clearRadius.</summary>
    private const float ClearRadius = 3f;

    /// <summary>
    ///     The whole chain, start to top. Pinning each rung on its own would let a mutation swap
    ///     two of them and still pass, so every path is walked all the way to vent soil.
    /// </summary>
    [TestMethod]
    public void EveryStartingGroundClimbsItsWholeChain() {
        AssertChain("SoftSand", "Sand", "Gravel", "Soil", "SoilRich", AshVentSoilLadder.TopRung);
        AssertChain("Mud", "Sand", "Gravel", "Soil", "SoilRich", AshVentSoilLadder.TopRung);
        AssertChain("Sand", "Gravel", "Soil", "SoilRich", AshVentSoilLadder.TopRung);
        AssertChain("Gravel", "Soil", "SoilRich", AshVentSoilLadder.TopRung);
        AssertChain("Soil", "SoilRich", AshVentSoilLadder.TopRung);
        AssertChain("MossyTerrain", "SoilRich", AshVentSoilLadder.TopRung);
        AssertChain("MarshyTerrain", "SoilRich", AshVentSoilLadder.TopRung);
        AssertChain("SoilRich", AshVentSoilLadder.TopRung);
        AssertChain("Riverbank", AshVentSoilLadder.TopRung);
    }

    /// <summary>Our own soil is the top. Nothing above it, and the vent stops working the cell.</summary>
    [TestMethod]
    public void TheChainStopsAtVentSoil() {
        Assert.IsNull(
            AshVentSoilLadder.NextRung(AshVentSoilLadder.TopRung), "vent soil has a rung above it."
        );
    }

    /// <summary>
    ///     Warmth and nutrients improve ground. Bare rock is not ground, ice would melt rather than
    ///     enrich, and a floor the colony laid stays theirs.
    /// </summary>
    [TestMethod]
    public void GroundTheVentCannotFeedIsLeftAlone() {
        string[] refused = [
            "Sandstone_Rough", "Sandstone_RoughHewn", "Sandstone_Smooth", "Granite_Rough",
            "Granite_Smooth", "Marble_RoughHewn", "Limestone_Rough", "Slate_Smooth",
            "Ice", "ThinIce",
            "WaterShallow", "WaterDeep", "WaterOceanShallow", "WaterMovingShallow", "Marsh",
            "Concrete", "AncientConcrete", "MetalTile", "WoodPlankFloor", "PavedTile", "Bridge",
            "Cosmere_Scadrial_Terrain_Ash",
        ];

        foreach (string terrain in refused) {
            Assert.IsNull(AshVentSoilLadder.NextRung(terrain), $"{terrain} was put on the ladder.");
        }
    }

    /// <summary>A terrain the mod has never heard of is left alone, not defaulted into the chain.</summary>
    [TestMethod]
    public void TerrainFromAnotherModIsNeverTouched() {
        Assert.IsNull(AshVentSoilLadder.NextRung("SomeOtherMod_Terrain_Loam"));
        Assert.IsNull(AshVentSoilLadder.NextRung(string.Empty));
        Assert.IsNull(AshVentSoilLadder.NextRung(null));
    }

    /// <summary>
    ///     Every rung has to land on ground the chain knows, or a cell reaches a name nothing
    ///     carries on from and stalls one step short of the top.
    /// </summary>
    [TestMethod]
    public void EveryRungLandsOnTheChainOrTheTop() {
        foreach (KeyValuePair<string, string> rung in AshVentSoilLadder.Rungs) {
            if (rung.Value == AshVentSoilLadder.TopRung) continue;

            Assert.IsTrue(
                AshVentSoilLadder.Rungs.ContainsKey(rung.Value),
                $"{rung.Key} climbs to {rung.Value}, which the chain does not carry on from."
            );
        }
    }

    /// <summary>Nothing climbs to itself, which would spend the sweep's ration on no change at all.</summary>
    [TestMethod]
    public void NoRungClimbsToItself() {
        foreach (KeyValuePair<string, string> rung in AshVentSoilLadder.Rungs) {
            Assert.AreNotEqual(rung.Key, rung.Value, $"{rung.Key} climbs to itself.");
        }
    }

    /// <summary>Aaron's rate, and the numbers he read off it: sand to vent soil in thirty-two days.</summary>
    [TestMethod]
    public void ARungTakesEightDays() {
        Assert.AreEqual(8f, AshVentSoilLadder.DaysPerRung);
        Assert.AreEqual(32f, RungsFrom("Sand") * AshVentSoilLadder.DaysPerRung, "sand is not four rungs out.");
        Assert.AreEqual(16f, 2 * AshVentSoilLadder.DaysPerRung, "gravel to rich soil is not two rungs.");
        Assert.AreEqual(2, RungsFrom("Gravel") - 1, "gravel is not two rungs under rich soil.");
    }

    [TestMethod]
    public void ARungIsOnlyEarnedOnceItsDwellIsDone() {
        Assert.AreEqual(0, AshVentSoilLadder.RungsClimbed(-40f));
        Assert.AreEqual(0, AshVentSoilLadder.RungsClimbed(0f));
        Assert.AreEqual(0, AshVentSoilLadder.RungsClimbed(7.99f));
        Assert.AreEqual(1, AshVentSoilLadder.RungsClimbed(8f));
        Assert.AreEqual(1, AshVentSoilLadder.RungsClimbed(15.99f));
        Assert.AreEqual(4, AshVentSoilLadder.RungsClimbed(32f));
    }

    /// <summary>The clearing is the vent's from the day it spawns, so its dwell starts at nothing.</summary>
    [TestMethod]
    public void TheClearingStartsItsDwellImmediately() {
        Assert.AreEqual(0f, AshVentSoilLadder.WarmedAtDays(0f, ClearRadius), 0.0001f);
        Assert.AreEqual(0f, AshVentSoilLadder.WarmedAtDays(ClearRadius, ClearRadius), 0.0001f);
    }

    /// <summary>
    ///     Ground past the clearing waits for the front, which creeps a cell every DaysPerCell. A
    ///     cell that climbed before the vent reached it would be enriching itself.
    /// </summary>
    [TestMethod]
    public void GroundPastTheClearingWaitsForTheFront() {
        Assert.AreEqual(
            2f * AshVentSoilSpread.DaysPerCell,
            AshVentSoilLadder.WarmedAtDays(ClearRadius + 2f, ClearRadius),
            0.0001f,
            "the wait is no longer counted from the edge of the clearing, so ground the front already holds is made "
            + "to wait for it all over again."
        );

        for (float distance = ClearRadius; distance <= 16f; distance += 0.25f) {
            float front = AshVentSoilSpread.Advance(0f, ClearRadius, 16f, AshVentSoilLadder.WarmedAtDays(distance, ClearRadius));

            Assert.IsTrue(
                front >= distance - 0.0001f,
                $"a cell {distance} out starts its dwell at day "
                + $"{AshVentSoilLadder.WarmedAtDays(distance, ClearRadius)}, when the front is only at {front}."
            );
        }
    }

    /// <summary>
    ///     Sand in the clearing reaches vent soil in four rungs plus its own jitter, and not one
    ///     rung sooner. Counted the way the comp counts it, a cycle at a time.
    /// </summary>
    [TestMethod]
    public void SandInTheClearingTakesFourRungsToReachVentSoil() {
        const int x = 40;
        const int z = 71;

        List<float> climbed = [];
        float days = 0f;
        float jitter = AshVentSoilLadder.JitterDays(x, z);

        for (int cycle = 0; cycle < (int)(72f / CycleDays); cycle++) {
            float was = days;
            days += CycleDays;
            if (AshVentSoilLadder.RungIsDue(x, z, 0f, ClearRadius, was, days)) climbed.Add(days);
        }

        Assert.IsTrue(jitter < AshVentSoilLadder.DaysPerRung, $"the jitter {jitter} is over a whole rung.");
        Assert.IsTrue(climbed.Count >= RungsFrom("Sand"), $"seventy-two days bought only {climbed.Count} rungs.");

        for (int rung = 1; rung <= RungsFrom("Sand"); rung++) {
            float expected = jitter + (rung * AshVentSoilLadder.DaysPerRung);
            Assert.AreEqual(expected, climbed[rung - 1], 0.01f, $"rung {rung} did not land at day {expected}.");
        }

        Assert.AreEqual(32f, RungsFrom("Sand") * AshVentSoilLadder.DaysPerRung, "sand is no longer 32 days out.");
    }

    /// <summary>
    ///     The reason the jitter exists. The front takes a cell in the time a rung takes, so without
    ///     it every whole-numbered ring comes due on one tick and the sweep's ration eats the rest.
    /// </summary>
    [TestMethod]
    public void NoOneSweepEverOwesMoreRungsThanItCanWrite() {
        List<(int x, int z, float distance)> patch = Patch(AshVentSoilSpread.DefaultReachCells);
        int worst = 0;
        float days = 0f;

        // A full pass of the front out to the default eight, plus a rung window on the far side.
        for (int cycle = 0; cycle < (int)(90f / CycleDays); cycle++) {
            float was = days;
            days += CycleDays;

            int due = 0;
            foreach ((int x, int z, float distance) cell in patch) {
                if (AshVentSoilLadder.RungIsDue(cell.x, cell.z, cell.distance, ClearRadius, was, days)) due++;
            }

            if (due > worst) worst = due;
        }

        Assert.IsTrue(
            worst <= AshDepthMath.TerrainChangesPerSweep,
            $"one sweep owed {worst} rungs against a ration of {AshDepthMath.TerrainChangesPerSweep}."
        );
    }

    /// <summary>
    ///     The jitter has to actually scatter. A hash that collapsed onto a handful of values would
    ///     pass every rung test above and still stack a ring onto one tick.
    /// </summary>
    [TestMethod]
    public void TheJitterSpreadsAcrossTheWholeRungWindow() {
        HashSet<int> buckets = [];

        for (int x = -20; x <= 20; x++) {
            for (int z = -20; z <= 20; z++) {
                float jitter = AshVentSoilLadder.JitterDays(x, z);

                Assert.IsTrue(
                    jitter >= 0f && jitter < AshVentSoilLadder.DaysPerRung,
                    $"cell {x},{z} jittered {jitter}, outside one rung window."
                );
                buckets.Add((int)jitter);
            }
        }

        Assert.AreEqual(
            (int)AshVentSoilLadder.DaysPerRung,
            buckets.Count,
            "the jitter never lands in some parts of the window."
        );
    }

    /// <summary>A cell's jitter is a fact about where it is, so a reload cannot shift its schedule.</summary>
    [TestMethod]
    public void TheJitterIsTheSameEveryTimeItIsAsked() {
        Assert.AreEqual(AshVentSoilLadder.JitterDays(13, -7), AshVentSoilLadder.JitterDays(13, -7));
        Assert.AreNotEqual(AshVentSoilLadder.JitterDays(13, -7), AshVentSoilLadder.JitterDays(-7, 13));
    }

    /// <summary>
    ///     The rung clock is banked state. A cell's rung is its terrain, but nothing on the ground
    ///     records when it last climbed, and this feature has shipped unsaved state twice already.
    /// </summary>
    [TestMethod]
    public void TheRungClockIsScribed() {
        string comp = Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Comp", "Thing", "CompAshVent.cs"
        );

        Assert.IsTrue(File.Exists(comp), $"CompAshVent.cs is missing at {comp}.");
        StringAssert.Contains(
            File.ReadAllText(comp),
            "Scribe_Values.Look(ref soilDays",
            "the rung clock is not saved, so every reload would push the whole patch a rung further out."
        );
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

    private static void AssertChain(params string[] expected) {
        string current = expected[0];

        for (int i = 1; i < expected.Length; i++) {
            string? next = AshVentSoilLadder.NextRung(current);

            Assert.AreEqual(expected[i], next, $"{current} climbs to {next ?? "nothing"}, not {expected[i]}.");
            current = next!;
        }

        Assert.IsNull(AshVentSoilLadder.NextRung(current), $"{expected[0]} climbs past {current}.");
    }

    private static int RungsFrom(string terrain) {
        int rungs = 0;
        string? current = terrain;

        while (AshVentSoilLadder.NextRung(current) is { } next) {
            rungs++;
            current = next;
        }

        return rungs;
    }

    /// <summary>Every cell a front of this radius covers around the 2x2 mouth, with its distance.</summary>
    private static List<(int x, int z, float distance)> Patch(float radius) {
        List<(int, int, float)> cells = [];
        int reach = (int)radius + 2;

        for (int x = -reach; x <= 1 + reach; x++) {
            for (int z = -reach; z <= 1 + reach; z++) {
                int dx = x < 0 ? -x : x > 1 ? x - 1 : 0;
                int dz = z < 0 ? -z : z > 1 ? z - 1 : 0;
                int distanceSquared = (dx * dx) + (dz * dz);
                if (!AshVentSoilSpread.Reaches(distanceSquared, radius)) continue;

                cells.Add((x, z, (float)Math.Sqrt(distanceSquared)));
            }
        }

        return cells;
    }
}
