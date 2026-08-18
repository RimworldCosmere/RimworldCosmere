using Cosmere.Core.ShardConnection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     What a pawn is credited with the first time the residence tracker ever sees them - age for
///     a native, nothing at all for anyone born somewhere else.
/// </summary>
[TestClass]
public class ResidenceSeedTests {
    [TestMethod]
    public void ANativeAdultArrivesFullySettled() {
        long thirtyYears = (long)ConnectionMath.TicksPerYear * 30;
        Assert.AreEqual(ConnectionMath.TicksToFullResidence, ConnectionMath.SeedTicksForAge(thirtyYears, true));
    }

    [TestMethod]
    public void ANativeChildArrivesPartlySettled() {
        long fiveYears = (long)ConnectionMath.TicksPerYear * 5;
        Assert.AreEqual((int)fiveYears, ConnectionMath.SeedTicksForAge(fiveYears, true));
    }

    // The drop-pod rule: where you were born decides this, not how old you are.
    [TestMethod]
    public void AnOffWorlderArrivesWithNothingHoweverOldTheyAre() {
        long sixtyYears = (long)ConnectionMath.TicksPerYear * 60;
        Assert.AreEqual(0, ConnectionMath.SeedTicksForAge(sixtyYears, false));
    }

    [TestMethod]
    public void NonsenseAgesSeedNothing() {
        Assert.AreEqual(0, ConnectionMath.SeedTicksForAge(-1, true));
        Assert.AreEqual(0, ConnectionMath.SeedTicksForAge(0, true));
    }
}
