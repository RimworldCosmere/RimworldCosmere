using System;
using System.Collections.Generic;
using System.Linq;
using Cosmere.System.Scadrial.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Covers how a metal's abilities are ordered and grouped in the dock strip.
/// </summary>
/// <remarks>
///     Kept free of RimWorld types on purpose - the test host has no Assembly-CSharp, so
///     anything touching AbilityDef has to stay on the drawing side.
/// </remarks>
[TestClass]
public class AbilityRowLayoutTests {
    private static AbilityRow Sustained(string name) {
        return new AbilityRow(name, name, false, true);
    }

    private static AbilityRow Targeted(string name) {
        return new AbilityRow(name, name, true, true);
    }

    [TestMethod]
    public void SustainedComeBeforeTargeted() {
        List<AbilityRow> ordered = AbilityRowLayout.Ordered([
            Sustained("SteelAura"),
            Targeted("SteelJump"),
            Sustained("SteelBubble"),
            Targeted("SteelPush"),
            Targeted("SteelCoinshot"),
        ]);

        CollectionAssert.AreEqual(
            new[] { "SteelAura", "SteelBubble", "SteelJump", "SteelPush", "SteelCoinshot" },
            ordered.Select(r => r.DefName).ToList()
        );
    }

    [TestMethod]
    public void OrderWithinAGroupIsPreserved() {
        List<AbilityRow> ordered = AbilityRowLayout.Ordered([
            Targeted("IronPull"),
            Sustained("IronAura"),
        ]);

        Assert.AreEqual("IronAura", ordered[0].DefName);
        Assert.AreEqual("IronPull", ordered[1].DefName);
    }

    [TestMethod]
    public void HeadersOnlyWhenBothKindsPresent() {
        Assert.IsTrue(AbilityRowLayout.ShowGroupHeaders([Sustained("SteelAura"), Targeted("SteelPush")]));
        Assert.IsFalse(AbilityRowLayout.ShowGroupHeaders([Sustained("CopperAura")]));
        Assert.IsFalse(AbilityRowLayout.ShowGroupHeaders([Targeted("Chromium")]));
        Assert.IsFalse(AbilityRowLayout.ShowGroupHeaders([]));
    }

    [TestMethod]
    public void HeightCountsRowsGapsAndHeaders() {
        float one = AbilityRowLayout.HeightFor([Sustained("CopperAura")]);
        Assert.AreEqual(AbilityRowLayout.RowHeight, one, 0.001f);

        float two = AbilityRowLayout.HeightFor([Sustained("CopperAura"), Sustained("Extra")]);
        Assert.AreEqual(AbilityRowLayout.RowHeight * 2f + AbilityRowLayout.RowGap, two, 0.001f);

        float grouped = AbilityRowLayout.HeightFor([Sustained("IronAura"), Targeted("IronPull")]);
        Assert.AreEqual(
            AbilityRowLayout.RowHeight * 2f + AbilityRowLayout.RowGap + AbilityRowLayout.GroupHeaderHeight * 2f,
            grouped,
            0.001f
        );
    }

    [TestMethod]
    public void EmptyIsZeroHigh() {
        Assert.AreEqual(0f, AbilityRowLayout.HeightFor([]), 0.001f);
    }
}
