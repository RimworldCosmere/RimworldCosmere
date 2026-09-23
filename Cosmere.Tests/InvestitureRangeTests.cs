using Cosmere.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     A container with a Stormlight band only takes spheres charged inside it. These pin where the
///     edges of that band sit.
/// </summary>
[TestClass]
public class InvestitureRangeTests {
    [TestMethod]
    public void AChargeInsideTheBandIsAllowed() {
        Assert.IsTrue(InvestitureRange.Within(50f, 100f, 0.25f, 0.75f), "half full sits inside a quarter to three quarters");
    }

    [TestMethod]
    public void TheEdgesOfTheBandAreInclusive() {
        Assert.IsTrue(InvestitureRange.Within(25f, 100f, 0.25f, 0.75f), "the bottom edge is allowed");
        Assert.IsTrue(InvestitureRange.Within(75f, 100f, 0.25f, 0.75f), "the top edge is allowed");
    }

    [TestMethod]
    public void AChargeOutsideTheBandIsRefused() {
        Assert.IsFalse(InvestitureRange.Within(10f, 100f, 0.25f, 0.75f), "too empty");
        Assert.IsFalse(InvestitureRange.Within(90f, 100f, 0.25f, 0.75f), "too full");
    }

    [TestMethod]
    public void TheBandIsAShareNotAnAmount() {
        // a chip and a broam hold wildly different amounts, and one setting must mean the same thing on both.
        Assert.IsTrue(InvestitureRange.Within(5f, 10f, 0.4f, 0.6f), "half of a small gem");
        Assert.IsTrue(InvestitureRange.Within(500f, 1000f, 0.4f, 0.6f), "half of a large one");
    }

    [TestMethod]
    public void SomethingThatHoldsNoInvestitureIsNeverRuledOut() {
        // Steel and wood reach the same filter on their way into the charger.
        Assert.IsTrue(InvestitureRange.Within(0f, 0f, 0.9f, 1f), "no maximum means nothing to measure");
    }

    [TestMethod]
    public void AnEndlessSourceCountsAsFull() {
        Assert.IsTrue(InvestitureRange.Within(float.PositiveInfinity, 100f, 0.9f, 1f), "infinite reads as full");
        Assert.IsFalse(InvestitureRange.Within(float.PositiveInfinity, 100f, 0f, 0.5f), "and so is refused by a low band");
    }

    [TestMethod]
    public void AFullBandTakesAnything() {
        Assert.IsTrue(InvestitureRange.Within(0f, 100f, 0f, 1f), "empty");
        Assert.IsTrue(InvestitureRange.Within(100f, 100f, 0f, 1f), "full");
    }
}
