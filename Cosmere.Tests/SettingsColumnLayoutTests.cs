using Cosmere.Core.Settings.Layout;
using global::System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

[TestClass]
public class SettingsColumnLayoutTests {
    [TestMethod]
    public void SectionsGoIntoTheShorterColumnInDeclarationOrder() {
        IReadOnlyList<SettingSectionPlacement> placements = SettingsColumnLayout.Place(
            [
                new SettingSectionMeasurement("one", 200f),
                new SettingSectionMeasurement("two", 80f),
                new SettingSectionMeasurement("three", 100f),
            ],
            400f,
            12f
        );

        Assert.AreEqual(0, placements[0].Column);
        Assert.AreEqual(1, placements[1].Column);
        Assert.AreEqual(1, placements[2].Column);
        Assert.AreEqual(92f, placements[2].Y);
    }

    [TestMethod]
    public void EmptyInputProducesNoPlacements() {
        IReadOnlyList<SettingSectionPlacement> placements = SettingsColumnLayout.Place([], 400f, 12f);

        Assert.AreEqual(0, placements.Count);
    }

    [TestMethod]
    public void OneSectionStartsInTheLeftColumnAtTheOrigin() {
        IReadOnlyList<SettingSectionPlacement> placements = SettingsColumnLayout.Place(
            [new SettingSectionMeasurement("only", 80f)],
            400f,
            12f
        );

        Assert.AreEqual(0, placements[0].Column);
        Assert.AreEqual(0f, placements[0].X);
        Assert.AreEqual(0f, placements[0].Y);
        Assert.AreEqual(400f, placements[0].Width);
        Assert.AreEqual(80f, placements[0].Height);
    }

    [TestMethod]
    public void HeightTiesPreferTheLeftColumn() {
        IReadOnlyList<SettingSectionPlacement> placements = SettingsColumnLayout.Place(
            [
                new SettingSectionMeasurement("first", 100f),
                new SettingSectionMeasurement("second", 100f),
                new SettingSectionMeasurement("third", 50f),
            ],
            400f,
            12f
        );

        Assert.AreEqual(0, placements[0].Column);
        Assert.AreEqual(1, placements[1].Column);
        Assert.AreEqual(0, placements[2].Column);
        Assert.AreEqual(112f, placements[2].Y);
    }

    [TestMethod]
    public void MaxContentHeightExcludesTheTrailingGap() {
        IReadOnlyList<SettingSectionPlacement> placements = SettingsColumnLayout.Place(
            [
                new SettingSectionMeasurement("left", 200f),
                new SettingSectionMeasurement("right", 80f),
                new SettingSectionMeasurement("right-more", 100f),
            ],
            400f,
            12f
        );
        float maxContentHeight = 0f;

        foreach (SettingSectionPlacement placement in placements) {
            float bottom = placement.Y + placement.Height;
            if (bottom > maxContentHeight) maxContentHeight = bottom;
        }

        Assert.AreEqual(200f, maxContentHeight);
    }
}
