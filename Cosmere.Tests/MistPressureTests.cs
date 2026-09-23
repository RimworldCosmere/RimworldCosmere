using Cosmere.System.Scadrial.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Covers the mists' snap pressure, the dial a Hero of Ages progression beat turns up.
/// </summary>
/// <remarks>
///     The value used to be a bare static on MistsWatcher whose doc comment claimed it reset on
///     load. Nothing reset it, so cranking the mists to one-in-five and then loading a Final
///     Empire save in the same session left that colony playing at Hero of Ages odds.
///     MistPressureTracker resets this in its constructor and scribes it per save; the reset and
///     clamp live here because the tracker itself needs Verse and cannot run in this project.
/// </remarks>
[TestClass]
public class MistPressureTests {
    [TestInitialize]
    public void ResetBetweenTests() {
        MistPressure.Reset();
    }

    [TestMethod]
    public void StartsAtTheBaselineOdds() {
        Assert.AreEqual(16, MistPressure.Default);
        Assert.AreEqual(MistPressure.Default, MistPressure.OneIn);
    }

    [TestMethod]
    public void ResetReturnsToBaselineAfterAProgressionBeat() {
        MistPressure.OneIn = 5;
        Assert.AreEqual(5, MistPressure.OneIn);

        MistPressure.Reset();

        Assert.AreEqual(MistPressure.Default, MistPressure.OneIn);
    }

    [TestMethod]
    public void ClampsValuesBelowOne() {
        MistPressure.OneIn = 0;
        Assert.AreEqual(1, MistPressure.OneIn);

        MistPressure.OneIn = -12;
        Assert.AreEqual(1, MistPressure.OneIn);
    }

    [TestMethod]
    public void KeepsValuesAtOrAboveOne() {
        MistPressure.OneIn = 1;
        Assert.AreEqual(1, MistPressure.OneIn);

        MistPressure.OneIn = 30;
        Assert.AreEqual(30, MistPressure.OneIn);
    }
}
