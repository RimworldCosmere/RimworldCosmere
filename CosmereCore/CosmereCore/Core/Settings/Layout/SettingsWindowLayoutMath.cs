namespace Cosmere.Core.Settings.Layout;

public static class SettingsWindowLayoutMath {
    public const float SidebarWidth = 200f;
    public const float TitleHeight = 40f;

    // Tall enough that the 34f sigil and the 32f close mark both clear their padding.
    public const float CrestHeight = 52f;

    /// <summary>
    ///     Mirrors Verse.TabDrawer.TabHeight rather than referencing it, so this file stays pure
    ///     math the off-game test project can load.
    /// </summary>
    public const float SectionTabHeight = 32f;

    // Holds the 20f of air above the button row plus the row itself plus a bottom rest.
    public const float FooterHeight = 62f;
    public const float Gap = 12f;
    public const float ScrollbarWidth = 20f;

    // Air on each side of the drawn divider between the sidebar and the pane.
    public const float SidebarGutter = 16f;

    public const float PreferredWidth = 1200f;
    public const float PreferredHeight = 1000f;

    // Leaves the window clear of the screen edge on both sides at small resolutions.
    public const float ScreenInset = 40f;

    public static float PreferredWindowWidth(float screenWidth) {
        return global::System.Math.Min(PreferredWidth, screenWidth - ScreenInset);
    }

    public static float PreferredWindowHeight(float screenHeight) {
        return global::System.Math.Min(PreferredHeight, screenHeight - ScreenInset);
    }

    public static SettingsWindowLayoutData Create(float x, float y, float width, float height) {
        // The divider sits in the middle of this gutter, so neither the sidebar rows nor the pane content crowd it.
        float mainX = x + SidebarWidth + SidebarGutter;
        float mainWidth = global::System.Math.Max(0f, x + width - mainX);
        float footerY = y + height - FooterHeight;

        float sectionRailY = y + CrestHeight;
        float contentY = sectionRailY + SectionTabHeight;
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
