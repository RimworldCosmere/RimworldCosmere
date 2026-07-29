namespace Cosmere.Core.Settings.Layout;

public static class SettingsWindowLayoutMath {
    public const float SidebarWidth = 200f;
    public const float TitleHeight = 40f;
    public const float CrestHeight = 44f;

    // Verse.TabDrawer.TabHeight, mirrored rather than referenced so this file stays
    // pure math the off-game test project can load.
    public const float SectionTabHeight = 32f;
    public const float FooterHeight = 42f;
    public const float Gap = 12f;
    public const float ScrollbarWidth = 20f;

    public static SettingsWindowLayoutData Create(float x, float y, float width, float height) {
        // Sidebar and pane share an edge and are separated by a drawn border rather than
        // a gap, so the two regions read as one surface instead of two floating panels.
        float mainX = x + SidebarWidth;
        float mainWidth = global::System.Math.Max(0f, x + width - mainX);
        float footerY = y + height - FooterHeight;

        // TabDrawer.DrawTabs draws UPWARD: it shifts the rect it is handed up by 32 and
        // opens a 9999-tall group there, so anything already drawn in that band gets
        // painted over. The tab strip therefore gets its own reserved height below the
        // crest, and the rect handed to DrawTabs sits at the bottom of that band.
        float sectionRailY = y + CrestHeight + SectionTabHeight;
        float contentY = sectionRailY;
        float contentHeight = global::System.Math.Max(0f, footerY - contentY);
        float contentWidth = global::System.Math.Max(0f, mainWidth - ScrollbarWidth);

        return new SettingsWindowLayoutData(
            mainX,
            y,
            mainX,
            sectionRailY,
            mainX,
            contentY,
            mainWidth,
            contentHeight,
            mainX,
            contentY,
            contentWidth,
            contentHeight,
            x,
            footerY
        );
    }
}
