using Cosmere.Core.Settings.Layout;
using UnityEngine;

namespace Cosmere.Core.Window;

public readonly record struct SettingsWindowLayout(
    Rect Sidebar,
    Rect Crest,
    Rect SectionRail,
    Rect ContentViewport,
    Rect Content,
    Rect Footer
) {
    public const float SidebarWidth = SettingsWindowLayoutMath.SidebarWidth;
    public const float TitleHeight = SettingsWindowLayoutMath.TitleHeight;
    public const float CrestHeight = SettingsWindowLayoutMath.CrestHeight;
    public const float SectionTabHeight = SettingsWindowLayoutMath.SectionTabHeight;
    public const float FooterHeight = SettingsWindowLayoutMath.FooterHeight;
    public const float Gap = SettingsWindowLayoutMath.Gap;
    public const float ContentPadding = 14f;
    public const float ScrollbarWidth = SettingsWindowLayoutMath.ScrollbarWidth;

    public static SettingsWindowLayout Create(Rect inRect) {
        return Create(
            inRect.x,
            inRect.y,
            inRect.width,
            inRect.height
        );
    }

    public static SettingsWindowLayout Create(float x, float y, float width, float height) {
        SettingsWindowLayoutData layout = SettingsWindowLayoutMath.Create(x, y, width, height);

        return new SettingsWindowLayout(
            new Rect(x, y, SidebarWidth, layout.FooterY - y),
            new Rect(layout.CrestX, layout.CrestY, layout.ContentViewportWidth, CrestHeight),

            // DrawTabs shifts this rect up by 32 and draws there, so it is handed a
            // zero-height line at the bottom of the reserved band rather than the band.
            new Rect(layout.SectionRailX, layout.SectionRailY, layout.ContentViewportWidth, 0f),
            new Rect(
                layout.ContentViewportX,
                layout.ContentViewportY,
                layout.ContentViewportWidth,
                layout.ContentViewportHeight
            ),
            new Rect(layout.ContentX, layout.ContentY, layout.ContentWidth, layout.ContentHeight),
            new Rect(layout.FooterX, layout.FooterY, width, FooterHeight)
        );
    }
}
