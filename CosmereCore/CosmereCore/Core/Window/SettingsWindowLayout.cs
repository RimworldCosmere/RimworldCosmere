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
    public const float CrestHeight = SettingsWindowLayoutMath.CrestHeight;
    public const float SectionRailHeight = SettingsWindowLayoutMath.SectionRailHeight;
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
            new Rect(x, y, SidebarWidth, height),
            new Rect(layout.CrestX, y, layout.ContentViewportWidth, CrestHeight),
            new Rect(layout.SectionRailX, y + CrestHeight + Gap, layout.ContentViewportWidth, SectionRailHeight),
            new Rect(
                layout.ContentViewportX,
                layout.ContentViewportY,
                layout.ContentViewportWidth,
                layout.ContentViewportHeight
            ),
            new Rect(layout.ContentX, layout.ContentY, layout.ContentWidth, layout.ContentHeight),
            new Rect(layout.FooterX, layout.FooterY, layout.ContentViewportWidth, FooterHeight)
        );
    }
}
