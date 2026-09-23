using System.Collections.Generic;
using Cosmere.System.Scadrial.Feruchemy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The map arithmetic a metalmind uses to remember which duralumin ledger its charge came
///     from: total, rescale to a new total, and proportional drain.
/// </summary>
[TestClass]
public class ChargeAttributionTests {
    private const float Tolerance = 1e-3f;

    private static Dictionary<string, float> Map(float residence, float bonds, float shard) {
        return new Dictionary<string, float> {
            { "Residence", residence },
            { "Bonds", bonds },
            { "Shard", shard },
        };
    }

    [TestMethod]
    public void TotalSumsTheMap() {
        Assert.AreEqual(60f, ChargeAttribution.Total(Map(10f, 20f, 30f)), Tolerance);
    }

    [TestMethod]
    public void TotalOfAnEmptyOrNullMapIsZero() {
        Assert.AreEqual(0f, ChargeAttribution.Total(new Dictionary<string, float>()));
        Assert.AreEqual(0f, ChargeAttribution.Total(null));
    }

    [TestMethod]
    public void RescaleDownKeepsEachEntrysProportion() {
        Dictionary<string, float> map = Map(30f, 30f, 40f);

        ChargeAttribution.Rescale(map, 50f);

        Assert.AreEqual(15f, map["Residence"], Tolerance);
        Assert.AreEqual(15f, map["Bonds"], Tolerance);
        Assert.AreEqual(20f, map["Shard"], Tolerance);
        Assert.AreEqual(50f, ChargeAttribution.Total(map), Tolerance);
    }

    [TestMethod]
    public void RescaleToZeroEmptiesTheMap() {
        Dictionary<string, float> map = Map(30f, 30f, 40f);

        ChargeAttribution.Rescale(map, 0f);

        Assert.AreEqual(0, map.Count);
    }

    [TestMethod]
    public void RescaleUpIsProportional() {
        Dictionary<string, float> map = Map(10f, 0f, 0f);

        ChargeAttribution.Rescale(map, 40f);

        Assert.AreEqual(40f, map["Residence"], Tolerance);
        Assert.AreEqual(40f, ChargeAttribution.Total(map), Tolerance);
    }

    [TestMethod]
    public void RescaleFromAnEmptyMapDoesNotDivideByZero() {
        Dictionary<string, float> map = new Dictionary<string, float>();

        ChargeAttribution.Rescale(map, 50f);

        Assert.AreEqual(0, map.Count);
        Assert.AreEqual(0f, ChargeAttribution.Total(map), Tolerance);
    }

    [TestMethod]
    public void DrainRemovesExactlyTheAmountAskedSplitByShare() {
        Dictionary<string, float> map = Map(30f, 30f, 40f);

        float drained = ChargeAttribution.Drain(map, 20f);

        Assert.AreEqual(20f, drained, Tolerance);
        Assert.AreEqual(80f, ChargeAttribution.Total(map), Tolerance, "100 held minus the 20 drained.");
        Assert.AreEqual(24f, map["Residence"], Tolerance, "30 kept 30/100 of what remained after a 20-point drain.");
        Assert.AreEqual(24f, map["Bonds"], Tolerance);
        Assert.AreEqual(32f, map["Shard"], Tolerance);
    }

    [TestMethod]
    public void DrainingMoreThanTheMapHoldsRemovesEverythingAndReportsWhatMoved() {
        Dictionary<string, float> map = Map(10f, 10f, 0f);

        float drained = ChargeAttribution.Drain(map, 1000f);

        Assert.AreEqual(20f, drained, Tolerance, "Only the 20 the map held moved, not the 1000 asked.");
        Assert.AreEqual(0, map.Count);
    }

    [TestMethod]
    public void NothingGoesNegative() {
        Dictionary<string, float> map = Map(10f, 0f, 0f);

        ChargeAttribution.Rescale(map, -50f);
        Assert.AreEqual(0, map.Count, "A negative target clamps to zero and empties the map.");

        map = Map(10f, 0f, 0f);
        float drained = ChargeAttribution.Drain(map, -50f);
        Assert.AreEqual(0f, drained, "A negative ask drains nothing.");
        Assert.AreEqual(10f, map["Residence"], Tolerance);
    }

    [TestMethod]
    public void RoundTripConservesTheRemainder() {
        Dictionary<string, float> map = Map(30f, 30f, 40f);
        float start = ChargeAttribution.Total(map);

        float drained = ChargeAttribution.Drain(map, 25f);

        Assert.AreEqual(start - drained, ChargeAttribution.Total(map), Tolerance);
    }

    [TestMethod]
    public void DrainNamedTakesOnlyFromThatEntryWhenItHasEnough() {
        Dictionary<string, float> map = Map(30f, 30f, 40f);

        float drained = ChargeAttribution.DrainNamed(map, "Residence", 20f);

        Assert.AreEqual(20f, drained, Tolerance);
        Assert.AreEqual(10f, map["Residence"], Tolerance);
        Assert.AreEqual(30f, map["Bonds"], Tolerance);
        Assert.AreEqual(40f, map["Shard"], Tolerance);
    }

    [TestMethod]
    public void DrainNamedRemovesTheKeyWhenItExactlyEmptiesIt() {
        Dictionary<string, float> map = Map(30f, 30f, 40f);

        ChargeAttribution.DrainNamed(map, "Residence", 30f);

        Assert.IsFalse(map.ContainsKey("Residence"));
    }

    // Residence only holds 10 of the 50 asked; the other 40 drains from the rest, like Drain.
    [TestMethod]
    public void DrainNamedPullsAnyOverflowFromTheRestOfTheMapProportionally() {
        Dictionary<string, float> map = Map(10f, 30f, 90f);

        float drained = ChargeAttribution.DrainNamed(map, "Residence", 50f);

        Assert.AreEqual(50f, drained, Tolerance);
        Assert.IsFalse(map.ContainsKey("Residence"), "Residence's own 10 is gone entirely.");
        Assert.AreEqual(30f - 30f * (40f / 120f), map["Bonds"], Tolerance);
        Assert.AreEqual(90f - 90f * (40f / 120f), map["Shard"], Tolerance);
    }

    [TestMethod]
    public void DrainNamedNeverLeavesTheMapTotalAboveWhatWasRemoved() {
        Dictionary<string, float> map = Map(10f, 30f, 90f);
        float start = ChargeAttribution.Total(map);

        float drained = ChargeAttribution.DrainNamed(map, "Residence", 50f);

        Assert.AreEqual(start - drained, ChargeAttribution.Total(map), Tolerance);
    }

    [TestMethod]
    public void DrainNamedOnAnUnattributedKeyPullsEntirelyFromTheRest() {
        Dictionary<string, float> map = Map(0f, 40f, 60f);
        map.Remove("Residence");

        float drained = ChargeAttribution.DrainNamed(map, "Residence", 25f);

        Assert.AreEqual(25f, drained, Tolerance);
        Assert.IsFalse(map.ContainsKey("Residence"));
        Assert.AreEqual(75f, ChargeAttribution.Total(map), Tolerance);
    }

    [TestMethod]
    public void DrainNamedAskingMoreThanTheWholeMapHoldsReportsWhatMoved() {
        Dictionary<string, float> map = Map(10f, 10f, 0f);

        float drained = ChargeAttribution.DrainNamed(map, "Residence", 1000f);

        Assert.AreEqual(20f, drained, Tolerance, "Only the 20 the map held moved, not the 1000 asked.");
        Assert.AreEqual(0, map.Count);
    }

    [TestMethod]
    public void DrainNamedWithNothingToDrainIsANoOp() {
        Dictionary<string, float> map = Map(30f, 30f, 40f);

        Assert.AreEqual(0f, ChargeAttribution.DrainNamed(map, "Residence", 0f));
        Assert.AreEqual(0f, ChargeAttribution.DrainNamed(map, "Residence", -5f));
        Assert.AreEqual(30f, map["Residence"], Tolerance);
    }
}
