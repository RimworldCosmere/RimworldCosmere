using System.Collections.Generic;
using Cosmere.System.Scadrial.Grid;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The shape of what a vent puts on the ground. Inverse-square so drifts bank against the
///     vent, wind-skewed so the map develops a downwind side rather than a tidy circle.
/// </summary>
[TestClass]
public class AshPlumeTests {
    [TestMethod]
    public void TheVentCellGetsTheMost() {
        float centre = AshPlume.Weight(0, 0, 1f, 0f, 0f);
        Assert.IsTrue(centre > AshPlume.Weight(1, 0, 1f, 0f, 0f));
        Assert.IsTrue(centre > AshPlume.Weight(0, 1, 1f, 0f, 0f));
    }

    [TestMethod]
    public void WeightFallsOffWithDistance() {
        float near = AshPlume.Weight(2, 0, 1f, 0f, 0f);
        float mid = AshPlume.Weight(5, 0, 1f, 0f, 0f);
        float far = AshPlume.Weight(10, 0, 1f, 0f, 0f);

        Assert.IsTrue(near > mid, $"near {near} should beat mid {mid}");
        Assert.IsTrue(mid > far, $"mid {mid} should beat far {far}");
    }

    [TestMethod]
    public void NothingLandsPastTheRadius() {
        Assert.AreEqual(0f, AshPlume.Weight(AshPlume.RadiusCells + 1, 0, 1f, 0f, 0f), 0.0001f);
        Assert.AreEqual(0f, AshPlume.Weight(0, AshPlume.RadiusCells + 1, 1f, 0f, 0f), 0.0001f);
    }

    [TestMethod]
    public void WithNoSkewThePlumeIsSymmetric() {
        Assert.AreEqual(AshPlume.Weight(4, 0, 1f, 0f, 0f), AshPlume.Weight(-4, 0, 1f, 0f, 0f), 0.0001f);
        Assert.AreEqual(AshPlume.Weight(0, 4, 1f, 0f, 0f), AshPlume.Weight(0, -4, 1f, 0f, 0f), 0.0001f);
    }

    [TestMethod]
    public void SkewFavoursDownwind() {
        // Heading points along +x, so +x is downwind and -x is upwind.
        float downwind = AshPlume.Weight(5, 0, 1f, 0f, 0.6f);
        float upwind = AshPlume.Weight(-5, 0, 1f, 0f, 0.6f);

        Assert.IsTrue(downwind > upwind, $"downwind {downwind} should beat upwind {upwind}");
    }

    [TestMethod]
    public void SkewNeverProducesANegativeOrRunawayWeight() {
        for (int x = -AshPlume.RadiusCells; x <= AshPlume.RadiusCells; x++) {
            for (int z = -AshPlume.RadiusCells; z <= AshPlume.RadiusCells; z++) {
                float w = AshPlume.Weight(x, z, 1f, 0f, 1f);
                Assert.IsTrue(w >= 0f, $"({x},{z}) gave a negative weight {w}");
                Assert.IsTrue(w <= 1f, $"({x},{z}) gave {w}, above the centre weight");
            }
        }
    }

    [TestMethod]
    public void BankDepositsNothingBelowOneUnit() {
        float remainder = 0f;
        int deposit = AshPlume.Bank(ref remainder, 4f, 10);

        Assert.AreEqual(0, deposit);
        Assert.AreEqual(4f, remainder, 0.0001f);
    }

    [TestMethod]
    public void BankEventuallyDepositsAWholeMultipleOfTheUnit() {
        float remainder = 0f;
        int deposit = 0;
        for (int i = 0; i < 4; i++) {
            deposit = AshPlume.Bank(ref remainder, 3f, 10);
        }

        // 4 adds of 3mm = 12mm, which crosses the 10mm unit on the fourth call.
        Assert.AreEqual(10, deposit);
        Assert.AreEqual(0, deposit % 10);
        Assert.AreEqual(2f, remainder, 0.0001f);
    }

    [TestMethod]
    public void BankConservesMassOverManySmallAdds() {
        float remainder = 0f;
        int totalDeposited = 0;
        for (int i = 0; i < 50000; i++) {
            totalDeposited += AshPlume.Bank(ref remainder, 0.15f, 10);
        }

        // 50000 adds of 0.15mm = 7500mm total. A version that discards the remainder instead of
        // subtracting the deposit loses a sliver every crossing; over this many crossings the
        // slivers compound into whole missed units, landing well under 7480. Correct banking
        // never drops more than one unit's worth of not-yet-deposited change.
        Assert.IsTrue(totalDeposited is >= 7480 and <= 7500, $"deposited {totalDeposited}, expected 7480-7500");
    }

    [TestMethod]
    public void BankIgnoresZeroAndNegativeAdds() {
        float remainder = 5f;

        Assert.AreEqual(0, AshPlume.Bank(ref remainder, 0f, 10));
        Assert.AreEqual(5f, remainder, 0.0001f);

        Assert.AreEqual(0, AshPlume.Bank(ref remainder, -2f, 10));
        Assert.AreEqual(3f, remainder, 0.0001f);
    }

    [TestMethod]
    public void ARestoredBankKeepsEveryFractionItWasSavedWith() {
        float[] saved = new float[3];
        AshPlume.Bank(ref saved[0], 4f, 10);
        AshPlume.Bank(ref saved[1], 9.5f, 10);

        float[]? restored = AshPlume.RestoreBank([..saved], saved.Length);

        Assert.IsNotNull(restored);
        Assert.AreEqual(4f, restored[0], 0.0001f);
        Assert.AreEqual(9.5f, restored[1], 0.0001f);
        Assert.AreEqual(0f, restored[2], 0.0001f);

        // 9.5 was half a millimetre off a deposit; a bank dropped on load restarts the cell at 0.
        Assert.AreEqual(10, AshPlume.Bank(ref restored[1], 0.5f, 10));
    }

    [TestMethod]
    public void ABankSavedAtADifferentRadiusIsDiscardedRatherThanIndexedPast() {
        List<float> saved = [1f, 2f, 3f];

        Assert.IsNull(AshPlume.RestoreBank(saved, 4), "a grown offset list would read past the saved bank");
        Assert.IsNull(AshPlume.RestoreBank(saved, 2), "a shrunk one would leave the tail unread");
        Assert.IsNull(AshPlume.RestoreBank(null, 3), "a save from before the bank existed");
    }
}
