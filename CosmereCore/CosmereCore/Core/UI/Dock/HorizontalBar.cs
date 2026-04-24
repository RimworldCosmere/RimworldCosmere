using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Dock;

public static class HorizontalBar {
    public static void Draw(
        Rect rect,
        float fraction,
        float? targetFraction,
        Color backgroundColor,
        Color fillColor,
        Color targetMarkerColor
    ) {
        Widgets.DrawBoxSolid(rect, backgroundColor);

        float clampedFraction = Mathf.Clamp01(fraction);
        if (clampedFraction > 0f) {
            Rect fill = new Rect(rect.x, rect.y, rect.width * clampedFraction, rect.height);
            Widgets.DrawBoxSolid(fill, fillColor);
        }

        if (targetFraction.HasValue) {
            float t = Mathf.Clamp01(targetFraction.Value);
            float markerX = rect.x + rect.width * t;
            Rect marker = new Rect(markerX - 1f, rect.y, 2f, rect.height);
            Widgets.DrawBoxSolid(marker, targetMarkerColor);
        }

        Widgets.DrawBox(rect);
    }
}