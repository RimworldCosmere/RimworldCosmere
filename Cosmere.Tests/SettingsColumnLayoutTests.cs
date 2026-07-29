using Cosmere.Core.Settings.Layout;
using global::System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

[TestClass]
public class SettingsColumnLayoutTests {
    [TestMethod]
    public void SectionsGoIntoTheShorterColumnInDeclarationOrder() {
        SettingsColumnLayoutResult layout = SettingsColumnLayout.Place(
            [
                new SettingSectionMeasurement("one", 200f),
                new SettingSectionMeasurement("two", 80f),
                new SettingSectionMeasurement("three", 100f),
            ],
            400f,
            12f
        );

        Assert.AreEqual(0, layout.Placements[0].Column);
        Assert.AreEqual(1, layout.Placements[1].Column);
        Assert.AreEqual(1, layout.Placements[2].Column);
        Assert.AreEqual(92f, layout.Placements[2].Y);
    }

    [TestMethod]
    public void EmptyInputProducesNoPlacementsOrContentHeight() {
        SettingsColumnLayoutResult layout = SettingsColumnLayout.Place([], 400f, 12f);

        Assert.AreEqual(0, layout.Placements.Count);
        Assert.AreEqual(0f, layout.ContentHeight);
    }

    [TestMethod]
    public void OneSectionStartsInTheLeftColumnAtTheOrigin() {
        SettingsColumnLayoutResult layout = SettingsColumnLayout.Place(
            [new SettingSectionMeasurement("only", 80f)],
            400f,
            12f
        );

        Assert.AreEqual(0, layout.Placements[0].Column);
        Assert.AreEqual(0f, layout.Placements[0].X);
        Assert.AreEqual(0f, layout.Placements[0].Y);
        Assert.AreEqual(400f, layout.Placements[0].Width);
        Assert.AreEqual(80f, layout.Placements[0].Height);
        Assert.AreEqual(80f, layout.ContentHeight);
    }

    [TestMethod]
    public void HeightTiesPreferTheLeftColumn() {
        SettingsColumnLayoutResult layout = SettingsColumnLayout.Place(
            [
                new SettingSectionMeasurement("first", 100f),
                new SettingSectionMeasurement("second", 100f),
                new SettingSectionMeasurement("third", 50f),
            ],
            400f,
            12f
        );

        Assert.AreEqual(0, layout.Placements[0].Column);
        Assert.AreEqual(1, layout.Placements[1].Column);
        Assert.AreEqual(0, layout.Placements[2].Column);
        Assert.AreEqual(112f, layout.Placements[2].Y);
    }

    [TestMethod]
    public void ContentHeightUsesTheTallerColumnWithoutItsTrailingGap() {
        SettingsColumnLayoutResult layout = SettingsColumnLayout.Place(
            [
                new SettingSectionMeasurement("left", 200f),
                new SettingSectionMeasurement("right", 80f),
                new SettingSectionMeasurement("right-more", 100f),
            ],
            400f,
            12f
        );

        Assert.AreEqual(200f, layout.ContentHeight);
    }
}
