using Cosmere.System.Scadrial.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

[TestClass]
public class AshLungMathTests {
    [TestMethod]
    public void AFullyMaskedPawnNeverGainsSeverity() {
        Assert.IsLessThanOrEqualTo(0f, AshLungMath.SeverityDeltaPerHour(1f, 1f));
    }

    [TestMethod]
    public void AnUnmaskedPawnAtTheMouthGains() {
        Assert.IsGreaterThan(0f, AshLungMath.SeverityDeltaPerHour(1f, 0f));
    }

    [TestMethod]
    public void ClearAirRecedes() {
        Assert.IsLessThan(0f, AshLungMath.SeverityDeltaPerHour(0f, 0f));
    }

    [TestMethod]
    public void RecedingIsSlowerThanGaining() {
        float gain = AshLungMath.SeverityDeltaPerHour(1f, 0f);
        float recede = -AshLungMath.SeverityDeltaPerHour(0f, 0f);
        Assert.IsLessThan(gain, recede, $"gain {gain}, recede {recede}");
    }

    [TestMethod]
    public void PartialFiltrationSitsBetweenTheExtremes() {
        float none = AshLungMath.EffectiveExposure(1f, 0f);
        float half = AshLungMath.EffectiveExposure(1f, 0.5f);
        float full = AshLungMath.EffectiveExposure(1f, 1f);
        Assert.IsTrue(half < none && half > full, $"{none} / {half} / {full}");
    }

    [TestMethod]
    public void ExposureIsAlwaysInRange() {
        for (int i = 0; i <= 20; i++) {
            for (int j = 0; j <= 20; j++) {
                float e = AshLungMath.EffectiveExposure(i / 20f, j / 20f);
                Assert.IsTrue(e >= 0f && e <= 1f, $"raw {i / 20f} filt {j / 20f} gave {e}");
            }
        }
    }

    [TestMethod]
    public void OverFiltrationDoesNotInvertIntoHealing() {
        Assert.IsGreaterThanOrEqualTo(0f, AshLungMath.EffectiveExposure(1f, 5f));
    }
}
