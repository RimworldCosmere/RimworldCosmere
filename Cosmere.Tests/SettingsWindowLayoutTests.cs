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

        Assert.IsTrue(layout.ContentY + layout.ContentHeight <= layout.FooterY - SettingsWindowLayoutMath.Gap);
    }

    [TestMethod]
    public void MainSurfaceStartsAfterTheSystemSidebar() {
        SettingsWindowLayoutData layout = SettingsWindowLayoutMath.Create(20f, 40f, 900f, 700f);

        Assert.AreEqual(20f + SettingsWindowLayoutMath.SidebarWidth + SettingsWindowLayoutMath.Gap, layout.CrestX);
        Assert.AreEqual(layout.CrestX, layout.SectionRailX);
        Assert.AreEqual(layout.CrestX, layout.ContentViewportX);
        Assert.AreEqual(layout.CrestX, layout.FooterX);
    }
}
