using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     A coppercloud hides what stands in it from bronze, from rioting and from soothing. These pin
///     how several Smokers add up and where the line between hidden and seen sits.
/// </summary>
/// <remarks>
///     Only the arithmetic is here. Finding the clouds on a map needs a live Map and mapPawns, which
///     this project deliberately does not link, so <c>Coppercloud</c> itself is exercised in-game
///     against the scenario in BetaHub 11.
/// </remarks>
[TestClass]
public class CoppercloudContestTests {
    [TestMethod]
    public void NoCloudHidesNothing() {
        Assert.IsFalse(CoppercloudContest.Blocks(0f, 0f), "no cloud and no burn is still not hidden");
        Assert.IsFalse(CoppercloudContest.Blocks(0f, 3f), "a Seeker with nothing in the way sees");
    }

    [TestMethod]
    public void AStrongerCloudHides() {
        Assert.IsTrue(CoppercloudContest.Blocks(5f, 3f));
    }

    [TestMethod]
    public void AWeakerCloudDoesNot() {
        Assert.IsFalse(CoppercloudContest.Blocks(3f, 5f), "the Seeker burns through");
    }

    [TestMethod]
    public void ATieGoesToTheCloud() {
        // copper is defensive - a Smoker matching the Seeker exactly should win, or equal skill favors the attacker.
        Assert.IsTrue(CoppercloudContest.Blocks(4f, 4f));
    }

    [TestMethod]
    public void TwoWeakSmokersCanBeatOneSeeker() {
        // The point of the falloff being a half rather than nothing: stacking has to buy something.
        Assert.IsFalse(CoppercloudContest.Blocks(DiminishingStack.Combine([3f]), 4f));
        Assert.IsTrue(CoppercloudContest.Blocks(DiminishingStack.Combine([3f, 3f]), 4f));
    }

    [TestMethod]
    public void ADuraluminBurnClearsAnyOrdinaryStack() {
        // GetStrength already spikes tenfold on a duralumin burn - this is what that spike buys against three Smokers.
        Assert.IsFalse(CoppercloudContest.Blocks(DiminishingStack.Combine([4f, 4f, 4f]), 40f));
    }
}
