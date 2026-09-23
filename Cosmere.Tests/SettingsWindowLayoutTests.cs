using Cosmere.Core.Settings.Layout;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

[TestClass]
public class SettingsWindowLayoutTests {
    [TestMethod]
    public void ContentReservesTheFutureScrollbarWidth() {
        SettingsWindowLayoutData layout = SettingsWindowLayoutMath.Create(0f, 0f, 900f, 700f);

        Assert.AreEqual(SettingsWindowLayoutMath.ScrollbarWidth, layout.ContentViewportWidth - layout.ContentWidth);
        Assert.AreEqual(layout.ContentViewportX, layout.ContentX);
        Assert.AreEqual(layout.ContentViewportY, layout.ContentY);
        Assert.AreEqual(layout.ContentViewportHeight, layout.ContentHeight);
    }

    [TestMethod]
    public void ContentDoesNotOverlapTheFooter() {
        SettingsWindowLayoutData layout = SettingsWindowLayoutMath.Create(0f, 0f, 900f, 700f);

        Assert.IsTrue(layout.ContentY + layout.ContentHeight <= layout.FooterY);
    }

    [TestMethod]
    public void MainSurfaceClearsTheSidebarByTheGutter() {
        SettingsWindowLayoutData layout = SettingsWindowLayoutMath.Create(20f, 40f, 900f, 700f);

        // the divider rides the middle of this gutter, so the pane starts a full gutter past the sidebar.
        Assert.AreEqual(
            20f + SettingsWindowLayoutMath.SidebarWidth + SettingsWindowLayoutMath.SidebarGutter,
            layout.CrestX
        );
        Assert.AreEqual(layout.CrestX, layout.SectionRailX);
        Assert.AreEqual(layout.CrestX, layout.ContentViewportX);
    }

    [TestMethod]
    public void GutterLeavesRoomOnBothSidesOfTheDivider() {
        // a one-pixel divider centred in the gutter needs width so neither side reads crowded (pins audited spacing).
        Assert.IsTrue(SettingsWindowLayoutMath.SidebarGutter >= 12f);
    }

    [TestMethod]
    public void WindowTakesItsPreferredSizeWhenTheScreenHasRoom() {
        Assert.AreEqual(SettingsWindowLayoutMath.PreferredWidth, SettingsWindowLayoutMath.PreferredWindowWidth(2560f));
        Assert.AreEqual(SettingsWindowLayoutMath.PreferredHeight, SettingsWindowLayoutMath.PreferredWindowHeight(1440f));
    }

    [TestMethod]
    public void WindowStaysInsideASmallScreen() {
        // 1024x720 leaves less than the preferred size on both axes once the inset is taken, so the window must shrink.
        Assert.AreEqual(1024f - SettingsWindowLayoutMath.ScreenInset, SettingsWindowLayoutMath.PreferredWindowWidth(1024f));
        Assert.AreEqual(720f - SettingsWindowLayoutMath.ScreenInset, SettingsWindowLayoutMath.PreferredWindowHeight(720f));
    }

    [TestMethod]
    public void FooterLeavesAirOnBothSidesOfTheButtonRow() {
        // the 30f row is centred, so the footer needs slack that neither the divider nor the window edge crowds it.
        Assert.IsTrue((SettingsWindowLayoutMath.FooterHeight - 30f) / 2f >= 12f);
    }

    [TestMethod]
    public void FooterSpansTheFullWindowWidthFromItsLeftEdge() {
        SettingsWindowLayoutData layout = SettingsWindowLayoutMath.Create(20f, 40f, 900f, 700f);

        Assert.AreEqual(20f, layout.FooterX);
    }

    [TestMethod]
    public void HeaderRowsStackInPlaceAboveTheContent() {
        SettingsWindowLayoutData layout = SettingsWindowLayoutMath.Create(0f, 0f, 900f, 700f);

        // the rail draws inside the rect it's handed, so the rows simply stack: crest, then tabs, then content.
        Assert.AreEqual(SettingsWindowLayoutMath.CrestHeight, layout.SectionRailY - layout.CrestY);
        Assert.AreEqual(SettingsWindowLayoutMath.SectionTabHeight, layout.ContentY - layout.SectionRailY);
    }
}
