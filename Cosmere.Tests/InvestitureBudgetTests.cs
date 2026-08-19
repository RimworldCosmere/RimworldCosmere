using Cosmere.System.Scadrial.Feruchemy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The floor that stops nicrosil storing Investiture the pawn does not have, and the ceiling
///     that stops a tap overshooting. Without the floor a band filled to 900 off a pawn holding 1.
/// </summary>
[TestClass]
public class InvestitureBudgetTests {
    private const float Rate = 0.125f;
    private const float Tolerance = 1e-5f;

    [TestMethod]
    public void APawnCanGiveExactlyWhatTheyHold() {
        Assert.AreEqual(8f, InvestitureBudget.Storable(1f, Rate), Tolerance);
        Assert.AreEqual(0f, InvestitureBudget.Storable(0f, Rate), Tolerance);
    }

    // The bug this exists for: an empty pawn kept filling the band, minting 892 charge.
    [TestMethod]
    public void AnEmptyPawnCanGiveNothing() {
        Assert.AreEqual(0f, InvestitureBudget.Storable(0f, Rate));
        Assert.AreEqual(0f, InvestitureBudget.Storable(-3f, Rate));
    }

    [TestMethod]
    public void APawnCanTakeBackWhatFitsUnderTheirCeiling() {
        Assert.AreEqual(792f, InvestitureBudget.Tappable(1f, 100f, Rate), Tolerance);
        Assert.AreEqual(0f, InvestitureBudget.Tappable(100f, 100f, Rate), Tolerance);
    }

    [TestMethod]
    public void APawnOverTheirCeilingTakesNothingRatherThanNegative() {
        Assert.AreEqual(0f, InvestitureBudget.Tappable(150f, 100f, Rate));
    }

    /// <summary>
    ///     Store to the floor, tap it all back, land on exactly where you started. 0.125 is a power
    ///     of two, so both directions are exponent shifts and the round trip is exact, not near.
    /// </summary>
    [TestMethod]
    public void TheRoundTripReturnsTheStartingInvestitureExactly() {
        const float start = 1f;

        float banked = InvestitureBudget.Storable(start, Rate);
        float spent = InvestitureBudget.BeuFor(banked, Rate);
        Assert.AreEqual(start, spent);

        float afterStoring = start - spent;
        Assert.AreEqual(0f, afterStoring);

        float returned = InvestitureBudget.BeuFor(banked, Rate);
        Assert.AreEqual(start, afterStoring + returned);
    }

    /// <summary>
    ///     The last partial second: the pawn has less than one charge's worth left, so the tick's
    ///     full ask has to clip to the budget rather than overdraw and let a clamp eat the rest.
    /// </summary>
    [TestMethod]
    public void ThePartialSecondClipsToWhatIsLeft() {
        float budget = InvestitureBudget.Storable(0.05f, Rate);
        Assert.AreEqual(0.4f, budget, Tolerance);

        float asked = 1f;
        float moved = asked < budget ? asked : budget;
        Assert.AreEqual(0.4f, moved, Tolerance);
        Assert.AreEqual(0.05f, InvestitureBudget.BeuFor(moved, Rate), Tolerance);
    }

    [TestMethod]
    public void ANonsenseRateBudgetsNothingRatherThanDividingByZero() {
        Assert.AreEqual(0f, InvestitureBudget.Storable(10f, 0f));
        Assert.AreEqual(0f, InvestitureBudget.Tappable(0f, 100f, 0f));
        Assert.AreEqual(0f, InvestitureBudget.Storable(10f, -1f));
    }
}
