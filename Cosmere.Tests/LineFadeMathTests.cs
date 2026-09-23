using Cosmere.Core.Comp.Map;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Covers the alpha an allomantic metal line is drawn at.
/// </summary>
/// <remarks>
///     The radius is severity-driven, so a weak allomancer flaring lands it on exactly 3 and
///     used to divide by zero, handing FadedMaterialPool a fully transparent NaN alpha. That NaN
///     contributed to BetaHub issue 2, whose real cause was abilities collapsing to the first match.
/// </remarks>
[TestClass]
public class LineFadeMathTests {
    [TestMethod]
    public void RadiusOfExactlyThreeIsOpaqueRatherThanNaN() {
        float fade = LineFade.For(3f, 3f);

        Assert.IsFalse(float.IsNaN(fade), "A radius of exactly 3 must not produce a NaN alpha.");
        Assert.AreEqual(1f, fade, 0.0001f);
    }

    [TestMethod]
    public void RadiusBelowThreeIsOpaque() {
        Assert.AreEqual(1f, LineFade.For(1.5f, 0f), 0.0001f);
        Assert.AreEqual(1f, LineFade.For(1.5f, 1.5f), 0.0001f);
    }

    [TestMethod]
    public void NearThingsStayOpaqueAndFarThingsFadeToTheFloor() {
        Assert.AreEqual(1f, LineFade.For(15f, 3f), 0.0001f);
        Assert.AreEqual(1f, LineFade.For(15f, 1f), 0.0001f);
        Assert.AreEqual(0.5f, LineFade.For(15f, 9f), 0.0001f);
        Assert.AreEqual(0.3f, LineFade.For(15f, 15f), 0.0001f);
    }

    [TestMethod]
    public void FadeNeverDropsBelowTheFloor() {
        for (float distance = 0f; distance <= 40f; distance += 0.5f) {
            float fade = LineFade.For(20f, distance);
            Assert.IsFalse(float.IsNaN(fade), $"NaN alpha at distance {distance}.");
            Assert.IsTrue(fade >= 0.3f, $"Alpha {fade} fell below the 0.3 floor at distance {distance}.");
            Assert.IsTrue(fade <= 1f, $"Alpha {fade} exceeded 1 at distance {distance}.");
        }
    }
}
