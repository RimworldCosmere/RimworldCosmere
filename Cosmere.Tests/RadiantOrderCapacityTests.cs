using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Cosmere.Tests;

/// <summary>
///     Guards the Ideal capacity curve in the order source data.
/// </summary>
/// <remarks>
///     The Fourth Ideal shipped with no stormlightMax key at all in every one of the ten
///     orders. Surgebinder.MaxInvestitureLevel falls back to 1f when the value is not
///     positive, so a Fourth Ideal Radiant was capped at 1 Stormlight rather than 1500.
///     These read the JSON the defs are generated from, because that is where the gap was
///     and where a regeneration would reintroduce it.
/// </remarks>
[TestClass]
public class RadiantOrderCapacityTests {
    private static readonly int[] ExpectedCurve = [100, 300, 700, 1500, 3000];

    private static string OrdersDirectory {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "Resources", "Data", "RadiantOrders"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate Resources/Data/RadiantOrders above the test output directory.");
            return Path.Combine(dir.FullName, "Resources", "Data", "RadiantOrders");
        }
    }

    private static IEnumerable<object[]> OrderFiles() {
        foreach (string path in Directory.GetFiles(OrdersDirectory, "*.json")) {
            yield return [path];
        }
    }

    [TestMethod]
    public void EveryOrderFileIsFound() {
        Assert.AreEqual(10, Directory.GetFiles(OrdersDirectory, "*.json").Length, "Expected ten Radiant orders.");
    }

    [TestMethod]
    [DynamicData(nameof(OrderFiles))]
    public void EveryIdealDeclaresAPositiveCapacity(string path) {
        JArray ideals = IdealsOf(path);

        for (int i = 0; i < ideals.Count; i++) {
            JToken? max = ideals[i]["stormlightMax"];

            Assert.IsNotNull(
                max,
                $"{Path.GetFileName(path)} ideal {i + 1} has no stormlightMax. " +
                "MaxInvestitureLevel falls back to 1f, capping the Radiant at 1 Stormlight."
            );
            Assert.IsTrue(
                max.Value<int>() > 0,
                $"{Path.GetFileName(path)} ideal {i + 1} has stormlightMax {max.Value<int>()}, which must be positive."
            );
        }
    }

    [TestMethod]
    [DynamicData(nameof(OrderFiles))]
    public void CapacityRisesWithEveryIdeal(string path) {
        JArray ideals = IdealsOf(path);

        for (int i = 1; i < ideals.Count; i++) {
            int previous = ideals[i - 1]["stormlightMax"]!.Value<int>();
            int current = ideals[i]["stormlightMax"]!.Value<int>();

            Assert.IsTrue(
                current > previous,
                $"{Path.GetFileName(path)} ideal {i + 1} grants {current}, no more than ideal {i}'s {previous}. " +
                "Swearing an Oath must always raise the ceiling."
            );
        }
    }

    [TestMethod]
    [DynamicData(nameof(OrderFiles))]
    public void EveryOrderSharesTheSameCurve(string path) {
        JArray ideals = IdealsOf(path);
        Assert.AreEqual(ExpectedCurve.Length, ideals.Count, $"{Path.GetFileName(path)} does not have five Ideals.");

        for (int i = 0; i < ExpectedCurve.Length; i++) {
            Assert.AreEqual(
                ExpectedCurve[i],
                ideals[i]["stormlightMax"]!.Value<int>(),
                $"{Path.GetFileName(path)} ideal {i + 1} is off the shared capacity curve."
            );
        }
    }

    private static JArray IdealsOf(string path) {
        JObject order = JObject.Parse(File.ReadAllText(path));
        JArray? ideals = order["ideals"] as JArray;
        Assert.IsNotNull(ideals, $"{Path.GetFileName(path)} has no ideals array.");
        return ideals;
    }
}
