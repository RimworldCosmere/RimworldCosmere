using Cosmere.System.Scadrial.Feruchemy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards the one rule the whole system rests on: a metalmind hands back exactly what
///     went into it, at exactly the rate it took.
/// </summary>
[TestClass]
public class FeruchemyRateTests {
    private const float BandCapacity = 900f;
    private const float Tolerance = 1e-4f;

    // The bug this class exists for: storing and tapping used separate multipliers, so a
    // metal could be tuned to give back more than it took.
    [TestMethod]
    public void StoringAndTappingMoveChargeAtTheSameRate() {
        foreach (float severity in new[] { 1f, 5f, 12f, FeruchemyRate.MaxSeverity }) {
            float rate = FeruchemyRate.PerSecond(severity, 1f, 1f);

            Assert.AreEqual(
                FeruchemyRate.SecondsToMove(BandCapacity, severity, 1f, 1f),
                BandCapacity / rate,
                Tolerance,
                $"severity {severity}"
            );
        }
    }

    [TestMethod]
    public void AFullBandRunsFifteenMinutesAtTheTopOfTheDial() {
        float seconds = FeruchemyRate.SecondsToMove(BandCapacity, FeruchemyRate.MaxSeverity, 1f, 1f);

        Assert.AreEqual(900f, seconds, 0.5f);
    }

    [TestMethod]
    public void TheDialStopsAtOneUnitPerSecond() {
        Assert.AreEqual(1f, FeruchemyRate.PerSecond(FeruchemyRate.MaxSeverity, 1f, 1f), Tolerance);
    }

    [TestMethod]
    public void ADeadDialMovesNothing() {
        Assert.AreEqual(0f, FeruchemyRate.PerSecond(0f, 1f, 1f));
        Assert.AreEqual(0f, FeruchemyRate.PerSecond(-3f, 1f, 1f));
    }

    [TestMethod]
    public void APerMetalMultiplierPacesBothDirectionsTogether() {
        float paced = FeruchemyRate.PerSecond(FeruchemyRate.MaxSeverity, 0.15f, 1f);

        Assert.AreEqual(0.15f, paced, Tolerance);
        Assert.AreEqual(6000f, FeruchemyRate.SecondsToMove(BandCapacity, FeruchemyRate.MaxSeverity, 0.15f, 1f), 1f);
    }

    [TestMethod]
    public void ANoviceGetsNoDiscount() {
        Assert.AreEqual(1f, FeruchemyRate.Efficiency(0f, 0), Tolerance);
        Assert.AreEqual(1f, FeruchemyRate.Efficiency(1f, 0), Tolerance);
        Assert.AreEqual(1f, FeruchemyRate.Efficiency(0f, 20), Tolerance);
    }

    [TestMethod]
    public void AMasterHalvesWhatTheySpend() {
        Assert.AreEqual(FeruchemyRate.MaxEfficiency, FeruchemyRate.Efficiency(1f, 20), Tolerance);
    }

    [TestMethod]
    public void SavantRankMultipliesOnTopOfSkill() {
        float plain = FeruchemyRate.Efficiency(1f, 20);
        float savant = FeruchemyRate.Efficiency(1f, 20, 1.6f);

        Assert.AreEqual(plain * 1.6f, savant, Tolerance);
    }

    [TestMethod]
    public void EfficiencyNeverPunishesTheSkilled() {
        float previous = 0f;
        for (int level = 0; level <= FeruchemyRate.MaxSkillLevel; level++) {
            float efficiency = FeruchemyRate.Efficiency(1f, level);
            Assert.IsTrue(efficiency >= previous, $"level {level} went backwards");
            previous = efficiency;
        }
    }

    // Efficiency buys duration, not magnitude - and it has to slow filling by the same
    // factor, or a savant would be pulling charge out of nowhere.
    [TestMethod]
    public void EfficiencyStretchesFillingAndDrawingEqually() {
        float efficiency = FeruchemyRate.Efficiency(1f, 20, 1.6f);
        float plain = FeruchemyRate.SecondsToMove(BandCapacity, FeruchemyRate.MaxSeverity, 1f, 1f);
        float skilled = FeruchemyRate.SecondsToMove(BandCapacity, FeruchemyRate.MaxSeverity, 1f, efficiency);

        Assert.AreEqual(plain * efficiency, skilled, Tolerance);
    }

    [TestMethod]
    public void OutOfRangeInputsAreClampedRatherThanExploding() {
        Assert.AreEqual(FeruchemyRate.MaxEfficiency, FeruchemyRate.Efficiency(9f, 999), Tolerance);
        Assert.AreEqual(1f, FeruchemyRate.Efficiency(-4f, -7), Tolerance);
        Assert.AreEqual(1f, FeruchemyRate.Efficiency(1f, 20, 0.1f) / FeruchemyRate.MaxEfficiency, Tolerance);
    }
}
