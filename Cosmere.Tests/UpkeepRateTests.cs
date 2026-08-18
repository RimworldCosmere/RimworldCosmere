using Cosmere.Core.Investiture;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Covers the rate arithmetic behind the dock's burn readout and the shared upkeep cadence.
/// </summary>
/// <remarks>
///     Kept free of RimWorld types on purpose - the test host has no Assembly-CSharp, so the
///     tick constants are mirrored here rather than read off GenTicks.
/// </remarks>
[TestClass]
public class UpkeepRateTests {
    private const float BreathEquivalentUnitsPerMetalUnit = 0.3125f;
    private const float DefaultBeuPerTick = 0.00166666666666666666f;
    private const float SteelAuraBeuPerTick = 0.0004f;

    [TestMethod]
    public void TicksForMapsEachCadence() {
        Assert.AreEqual(1, UpkeepRate.TicksFor(UpkeepCadence.PerTick));
        Assert.AreEqual(60, UpkeepRate.TicksFor(UpkeepCadence.PerSecond));
        Assert.AreEqual(250, UpkeepRate.TicksFor(UpkeepCadence.PerRareTick));
    }

    [TestMethod]
    public void PerSecondReadsStraightOnTheSecondCadence() {
        Assert.AreEqual(2f, UpkeepRate.PerSecond(2f, 60), 1e-6f);
    }

    [TestMethod]
    public void PerSecondScalesWithTheCadence() {
        Assert.AreEqual(120f, UpkeepRate.PerSecond(2f, 1), 1e-4f);
        Assert.AreEqual(0.48f, UpkeepRate.PerSecond(2f, 250), 1e-4f);
    }

    [TestMethod]
    public void PerSecondRejectsANonPositiveCadence() {
        Assert.AreEqual(0f, UpkeepRate.PerSecond(2f, 0));
        Assert.AreEqual(0f, UpkeepRate.PerSecond(2f, -60));
    }

    /// <summary>
    ///     The bug this class exists for: every allomantic cost divided down to a rare tick lands
    ///     below 0.005, so a two-decimal readout could only ever print 0.00.
    /// </summary>
    [TestMethod]
    public void TheCheapestBurnStaysVisibleAsAPercentage() {
        float percent = UpkeepRate.ReservePercentPerSecond(
            SteelAuraBeuPerTick,
            UpkeepRate.TicksFor(UpkeepCadence.PerSecond),
            BreathEquivalentUnitsPerMetalUnit,
            3.4594f
        );

        Assert.IsTrue(percent >= 0.005f, $"expected a printable percentage, got {percent}");
    }

    [TestMethod]
    public void EveryCadenceKeepsTheCheapestBurnPrintable() {
        foreach (UpkeepCadence cadence in new[] { UpkeepCadence.PerTick, UpkeepCadence.PerSecond, UpkeepCadence.PerRareTick }) {
            float percent = UpkeepRate.ReservePercentPerSecond(
                SteelAuraBeuPerTick,
                UpkeepRate.TicksFor(cadence),
                BreathEquivalentUnitsPerMetalUnit,
                3.4594f
            );

            Assert.IsTrue(percent >= 0.005f, $"{cadence} printed {percent}");
        }
    }

    [TestMethod]
    public void ReservePercentIsShareOfMaxPerSecond() {
        // A full reserve spent evenly over ten seconds is ten percent a second.
        float percent = UpkeepRate.ReservePercentPerSecond(0.3125f, 60, BreathEquivalentUnitsPerMetalUnit, 10f);

        Assert.AreEqual(10f, percent, 1e-4f);
    }

    [TestMethod]
    public void ReservePercentScalesWithConcurrentAbilities() {
        float one = UpkeepRate.ReservePercentPerSecond(DefaultBeuPerTick, 60, BreathEquivalentUnitsPerMetalUnit, 3f);
        float five = UpkeepRate.ReservePercentPerSecond(DefaultBeuPerTick * 5f, 60, BreathEquivalentUnitsPerMetalUnit, 3f);

        Assert.AreEqual(one * 5f, five, 1e-5f);
    }

    [TestMethod]
    public void ReservePercentIsZeroWithoutAReserve() {
        Assert.AreEqual(0f, UpkeepRate.ReservePercentPerSecond(DefaultBeuPerTick, 60, BreathEquivalentUnitsPerMetalUnit, 0f));
        Assert.AreEqual(0f, UpkeepRate.ReservePercentPerSecond(DefaultBeuPerTick, 60, BreathEquivalentUnitsPerMetalUnit, -1f));
    }

    [TestMethod]
    public void ReservePercentIsZeroWithoutAConversionRate() {
        Assert.AreEqual(0f, UpkeepRate.ReservePercentPerSecond(DefaultBeuPerTick, 60, 0f, 3f));
    }

    [TestMethod]
    public void AnIdleMetalReadsZero() {
        Assert.AreEqual(0f, UpkeepRate.ReservePercentPerSecond(0f, 60, BreathEquivalentUnitsPerMetalUnit, 3f));
    }
}
