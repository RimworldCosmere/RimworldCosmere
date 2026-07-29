namespace Cosmere.Core.Settings.Layout;

public static class SettingsWindowLayoutMath {
    public const float SidebarWidth = 200f;
    public const float CrestHeight = 44f;
    public const float SectionRailHeight = 44f;
    public const float FooterHeight = 42f;
    public const float Gap = 12f;
    public const float ScrollbarWidth = 20f;

    public static SettingsWindowLayoutData Create(float x, float y, float width, float height) {
        float mainX = x + SidebarWidth + Gap;
        float mainWidth = global::System.Math.Max(0f, x + width - mainX);
        float footerY = y + height - FooterHeight;
        float sectionRailY = y + CrestHeight + Gap;
        float contentY = sectionRailY + SectionRailHeight + Gap;
        float contentHeight = global::System.Math.Max(0f, footerY - Gap - contentY);
        float contentWidth = global::System.Math.Max(0f, mainWidth - ScrollbarWidth);

        return new SettingsWindowLayoutData(
            mainX,
            mainX,
            mainX,
            contentY,
            mainWidth,
            contentHeight,
            mainX,
            contentY,
            contentWidth,
            contentHeight,
            mainX,
            footerY
        );
    }
}
