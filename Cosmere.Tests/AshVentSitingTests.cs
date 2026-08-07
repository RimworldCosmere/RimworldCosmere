using Cosmere.System.Scadrial.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Where a vent may stand, and how many a map gets. Both are decisions the player cannot
///     undo once the map is generated, so they are worth pinning.
/// </summary>
[TestClass]
public class AshVentSitingTests {
    [TestMethod]
    public void WaterIsNeverAVent() {
        Assert.IsFalse(AshVentSiting.IsPlausible(0.9f, true, 0f, true));
    }

    [TestMethod]
    public void FarmlandIsNeverAVent() {
        Assert.IsFalse(
            AshVentSiting.IsPlausible(0.9f, true, AshVentSiting.MaxFertility + 0.01f, false),
            "a vent on the best soil on the map pre-ruins the farm before the player arrives"
        );
        Assert.IsTrue(AshVentSiting.IsPlausible(0.9f, true, AshVentSiting.MaxFertility, false));
    }

    [TestMethod]
    public void LowGroundIsNeverAVent() {
        Assert.IsFalse(AshVentSiting.IsPlausible(AshVentSiting.MinElevation - 0.01f, true, 0f, false));
        Assert.IsTrue(AshVentSiting.IsPlausible(AshVentSiting.MinElevation, true, 0f, false));
    }

    [TestMethod]
    public void AVentNeedsRockNearby() {
        Assert.IsFalse(AshVentSiting.IsPlausible(0.9f, false, 0f, false), "a vent in open soil reads as nothing");
    }

    [TestMethod]
    public void CountRisesWithExposureAndStaysInRange() {
        Assert.AreEqual(1, AshVentSiting.CountForExposure(1f), "an unexposed map still gets one");
        Assert.AreEqual(6, AshVentSiting.CountForExposure(3f), "a mount-adjacent map gets the full six");

        int previous = 0;
        for (float e = 1f; e <= 3f; e += 0.05f) {
            int count = AshVentSiting.CountForExposure(e);
            Assert.IsTrue(count >= 1 && count <= 6, $"exposure {e} gave {count} vents");
            Assert.IsTrue(count >= previous, $"exposure {e} dropped the count from {previous} to {count}");
            previous = count;
        }
    }

    [TestMethod]
    public void CountIsClampedAgainstNonsenseExposure() {
        Assert.AreEqual(1, AshVentSiting.CountForExposure(0f));
        Assert.AreEqual(1, AshVentSiting.CountForExposure(-5f));
        Assert.AreEqual(6, AshVentSiting.CountForExposure(99f));
    }
}
