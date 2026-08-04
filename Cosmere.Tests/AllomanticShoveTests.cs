using Cosmere.System.Scadrial.Allomancy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Covers which end of a steelpush or ironpull moves.
/// </summary>
/// <remarks>
///     Buildings carry no Mass statBase, so a wall reported 1kg, read lighter than the Allomancer,
///     and the mover flung the wall instead of the pawn. Thing.Position on a region-affecting thing
///     leaves reachability stale, which is why the pawn could not walk afterwards either.
/// </remarks>
[TestClass]
public class AllomanticShoveTests {
    private const float HumanMass = 60f;
    private const float BuildingReportedMass = 1f;

    [TestMethod]
    public void AWallAnchorsTheShoveInsteadOfFlyingOffIt() {
        float mass = AllomanticShove.EffectiveTargetMass(BuildingReportedMass, HumanMass, true);

        Assert.IsTrue(
            AllomanticShove.MovesCaster(mass, HumanMass),
            $"A wall resolved to {mass}kg against a {HumanMass}kg pawn, so the wall is what moves."
        );
    }

    [TestMethod]
    public void AnAnchorOutweighsEvenAHeavilyLadenPawn() {
        const float laden = HumanMass * 4f;
        float mass = AllomanticShove.EffectiveTargetMass(BuildingReportedMass, laden, true);

        Assert.IsTrue(AllomanticShove.MovesCaster(mass, laden));
    }

    [TestMethod]
    public void ALooseItemStillTakesTheShove() {
        float mass = AllomanticShove.EffectiveTargetMass(0.5f, HumanMass, false);

        Assert.AreEqual(0.5f, mass, 0.001f, "An unanchored thing must keep its measured mass.");
        Assert.IsFalse(AllomanticShove.MovesCaster(mass, HumanMass));
    }

    [TestMethod]
    public void SomethingGenuinelyHeavierMovesTheCaster() {
        float mass = AllomanticShove.EffectiveTargetMass(HumanMass * 2f, HumanMass, false);

        Assert.IsTrue(AllomanticShove.MovesCaster(mass, HumanMass));
    }

    [TestMethod]
    public void AnAnchorClearsTheCasterByAKnownMargin() {
        float mass = AllomanticShove.EffectiveTargetMass(BuildingReportedMass, HumanMass, true);

        Assert.AreEqual(HumanMass + AllomanticShove.AnchorMassSurplus, mass, 0.001f);
    }

    [TestMethod]
    public void TheAnchorSurplusIsWhatSizesTheLaunch() {
        // MoveThing turns the mass difference into tiles with massDifference / 3, so the surplus is
        // the only thing setting how far a pawn travels off a wall.
        float mass = AllomanticShove.EffectiveTargetMass(BuildingReportedMass, HumanMass, true);
        float tiles = (mass - HumanMass) / 3f;

        Assert.IsTrue(tiles > 1f, $"A shove off a wall carried the pawn {tiles} tiles.");
    }
}
