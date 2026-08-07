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
}
