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
    public void MainSurfaceButtsAgainstTheSystemSidebar() {
        SettingsWindowLayoutData layout = SettingsWindowLayoutMath.Create(20f, 40f, 900f, 700f);

        // No gap: the sidebar and the pane share an edge and are split by a drawn border.
        Assert.AreEqual(20f + SettingsWindowLayoutMath.SidebarWidth, layout.CrestX);
        Assert.AreEqual(layout.CrestX, layout.SectionRailX);
        Assert.AreEqual(layout.CrestX, layout.ContentViewportX);
    }

    [TestMethod]
    public void FooterSpansTheFullWindowWidthFromItsLeftEdge() {
        SettingsWindowLayoutData layout = SettingsWindowLayoutMath.Create(20f, 40f, 900f, 700f);

        Assert.AreEqual(20f, layout.FooterX);
    }

    [TestMethod]
    public void TabStripGetsItsOwnBandBelowTheCrest() {
        SettingsWindowLayoutData layout = SettingsWindowLayoutMath.Create(0f, 0f, 900f, 700f);

        // TabDrawer.DrawTabs shifts its rect up by one tab height before drawing, so the
        // band it draws into must start at or below the crest's bottom edge.
        float crestBottom = layout.CrestY + SettingsWindowLayoutMath.CrestHeight;
        Assert.IsTrue(
            layout.SectionRailY - SettingsWindowLayoutMath.SectionTabHeight >= crestBottom,
            $"Tabs would paint over the crest: strip draws from {layout.SectionRailY - SettingsWindowLayoutMath.SectionTabHeight}, crest ends at {crestBottom}."
        );
        Assert.AreEqual(layout.SectionRailY, layout.ContentY);
    }
}
