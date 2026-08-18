using Cosmere.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Several Allomancers laying the same effect on one pawn. These pin that stacking buys
///     something real without letting a colony reach any number it likes by adding bodies.
/// </summary>
[TestClass]
public class DiminishingStackTests {
    [TestMethod]
    public void OneSourceIsWorthItself() {
        Assert.AreEqual(4f, DiminishingStack.Combine([4f]), 0.0001f);
    }

    [TestMethod]
    public void NoSourcesAreWorthNothing() {
        Assert.AreEqual(0f, DiminishingStack.Combine([]), 0.0001f);
    }

    [TestMethod]
    public void ASecondSourceAddsHalfOfWhatItIsWorth() {
        Assert.AreEqual(6f, DiminishingStack.Combine([4f, 4f]), 0.0001f);
    }

    [TestMethod]
    public void EachFurtherSourceAddsHalfAgain() {
        // 4 + 2 + 1, so four Smokers never reach twice what one manages.
        Assert.AreEqual(7f, DiminishingStack.Combine([4f, 4f, 4f]), 0.0001f);
        Assert.IsTrue(DiminishingStack.Combine([4f, 4f, 4f, 4f, 4f]) < 8f, "the stack converges");
    }

    [TestMethod]
    public void TheStrongestSourceCountsFirst() {
        // otherwise the answer depends on source order, and the same colony measures differently after a save reload.
        float weakFirst = DiminishingStack.Combine([1f, 6f]);
        float strongFirst = DiminishingStack.Combine([6f, 1f]);

        Assert.AreEqual(strongFirst, weakFirst, 0.0001f);
        Assert.AreEqual(6.5f, strongFirst, 0.0001f);
    }

    [TestMethod]
    public void StackingAlwaysBeatsStandingAlone() {
        // guards against two Smokers reading as one, which is what a source set holding only one entry looked like.
        Assert.IsTrue(DiminishingStack.Combine([3f, 3f]) > DiminishingStack.Combine([3f]));
    }
}
