using Cosmere.Core.UI;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

// Buttons for the dock strips. Vanilla's tan gradient fights both shardworld
// palettes, so these take the section's own accent instead.
public static class DockChrome {
    public static bool Button(Rect rect, string label, bool enabled, Color accent) {
        bool over = enabled && Mouse.IsOver(rect);
        Widgets.DrawBoxSolid(
            rect,
            !enabled
                ? new Color(0.075f, 0.082f, 0.090f)
                : over
                    ? new Color(0.145f, 0.161f, 0.176f)
                    : new Color(0.106f, 0.118f, 0.129f)
        );
        Widgets.DrawBoxSolidWithOutline(
            rect,
            Color.clear,
            enabled ? accent : new Color(0.145f, 0.157f, 0.169f)
        );
        UIText.EllipsisLabel(
            rect,
            label,
            GameFont.Tiny,
            TextAnchor.MiddleCenter,
            enabled ? new Color(0.788f, 0.812f, 0.831f) : new Color(0.310f, 0.325f, 0.337f)
        );

        return enabled && Widgets.ButtonInvisible(rect);
    }
}
