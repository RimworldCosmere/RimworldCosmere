using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The worldgen rows are anchored from the bottom of vanilla's left column because vanilla
///     walks that column with a local cursor no injection can read. These check the arithmetic
///     stays clear of vanilla's rows in every DLC combination - the drawing itself can only be
///     verified in game.
/// </summary>
[TestClass]
public class WorldParamsRowTests {
    private const float RowPitch = 40f;

    // Page.GetMainRect: new Rect(0, 45, rect.width, rect.height - 38 - 45 - 17).
    private const float TitleHeight = 45f;
    private const float BottomButtons = 38f;
    private const float BottomPad = 17f;

    private static float ColumnHeight(float windowHeight) => windowHeight - BottomButtons - TitleHeight - BottomPad;

    /// <summary>
    ///     Vanilla's cursor: seed at 0, randomize +40, coverage +40, rainfall +40, temperature
    ///     +40, population +40, then optional landmark, pollution and advanced-settings rows.
    ///     The last row is 30 tall.
    /// </summary>
    private static float VanillaBottom(bool odyssey, bool biotech, bool tutorial) {
        float cursor = 200f;
        if (odyssey) cursor += RowPitch;
        if (biotech) cursor += RowPitch;
        if (!tutorial) cursor += RowPitch;
        return cursor + 30f;
    }

    private static IEnumerable<object[]> Combinations() {
        foreach (bool odyssey in new[] { false, true }) {
            foreach (bool biotech in new[] { false, true }) {
                foreach (bool tutorial in new[] { false, true }) {
                    yield return [odyssey, biotech, tutorial];
                }
            }
        }
    }

    [TestMethod]
    [DynamicData(nameof(Combinations))]
    public void OurRowsNeverOverlapVanillaRows(bool odyssey, bool biotech, bool tutorial) {
        // 768 is the shortest window RimWorld supports.
        foreach (float windowHeight in new[] { 768f, 900f, 1080f, 1440f }) {
            float column = ColumnHeight(windowHeight);
            float ourTop = column - RowPitch * 2;
            float vanillaBottom = VanillaBottom(odyssey, biotech, tutorial);

            Assert.IsTrue(
                ourTop >= vanillaBottom,
                $"At {windowHeight}px (odyssey={odyssey} biotech={biotech} tutorial={tutorial}) our rows " +
                $"start at y={ourTop} but vanilla's last row ends at y={vanillaBottom}."
            );
        }
    }

    /// <summary>Both rows have to fit inside the column, not just start inside it.</summary>
    [TestMethod]
    public void BothRowsFitInsideTheColumn() {
        foreach (float windowHeight in new[] { 768f, 900f, 1080f, 1440f }) {
            float column = ColumnHeight(windowHeight);
            float ourTop = column - RowPitch * 2;

            Assert.IsTrue(ourTop >= 0, $"At {windowHeight}px the column is only {column}px, too short for two rows.");
            Assert.IsTrue(ourTop + RowPitch * 2 <= column, $"At {windowHeight}px the rows overflow the column.");
        }
    }

    /// <summary>
    ///     The densest case is the one that matters, and it is the one the spec claimed had
    ///     278px free. Guard the number so a future vanilla row addition trips this.
    /// </summary>
    [TestMethod]
    public void DensestCaseHasRoomToSpare() {
        float column = ColumnHeight(1080f);
        float free = column - VanillaBottom(true, true, false);

        Assert.IsTrue(free >= RowPitch * 2, $"Only {free}px free below vanilla's rows, need {RowPitch * 2}.");
    }
}
