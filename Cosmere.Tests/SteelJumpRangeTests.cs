using Cosmere.System.Scadrial.Allomancy.Verb;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Covers how far a steel jump actually carries a pawn.
/// </summary>
/// <remarks>
///     The old formula multiplied the def's range by raw allomantic power, which floors at
///     0.1, so a fresh Misting jumped about two tiles out of a stated twelve.
/// </remarks>
[TestClass]
public class SteelJumpRangeTests {
    private const float Base = 12f;
    private const float HumanMass = 60f;

    [TestMethod]
    public void AFreshMistingClearsMoreThanTwoTiles() {
        float burning = SteelJumpRange.For(Base, 1, 0.1f, HumanMass);
        float flaring = SteelJumpRange.For(Base, 2, 0.1f, HumanMass);

        Assert.IsTrue(burning > 5f, $"A fresh Misting jumped only {burning} tiles while burning.");
        Assert.IsTrue(flaring > 10f, $"A fresh Misting jumped only {flaring} tiles while flaring.");
    }

    [TestMethod]
    public void FlaringDoublesTheJump() {
        Assert.AreEqual(
            SteelJumpRange.For(Base, 1, 0.75f, HumanMass) * 2f,
            SteelJumpRange.For(Base, 2, 0.75f, HumanMass),
            0.001f
        );
    }

    [TestMethod]
    public void MorePowerReachesFurther() {
        float weak = SteelJumpRange.For(Base, 2, 0.1f, HumanMass);
        float middling = SteelJumpRange.For(Base, 2, 0.75f, HumanMass);
        float strong = SteelJumpRange.For(Base, 2, 3.35f, HumanMass);

        Assert.IsTrue(middling > weak);
        Assert.IsTrue(strong > middling);
    }

    [TestMethod]
    public void HeavierPawnsJumpShorter() {
        float light = SteelJumpRange.For(Base, 2, 0.75f, HumanMass);
        float laden = SteelJumpRange.For(Base, 2, 0.75f, HumanMass * 2f);

        Assert.IsTrue(laden < light);
    }

    [TestMethod]
    public void AVeryLightPawnCannotJumpAbsurdlyFar() {
        float feather = SteelJumpRange.For(Base, 2, 0.75f, 1f);
        float capped = SteelJumpRange.For(Base, 2, 0.75f, HumanMass * 0.5f);

        Assert.AreEqual(capped, feather, 0.001f, "The mass divisor must be floored, not unbounded.");
    }
}
