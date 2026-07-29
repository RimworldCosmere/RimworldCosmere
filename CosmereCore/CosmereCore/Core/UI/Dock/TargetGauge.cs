using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Dock;

public readonly record struct GaugePalette(
    Color Track,
    Color Fill,
    Color Notch,
    Color NotchIdle
);

// A reserve gauge the player can also act on. The notch is the refill threshold,
// dragged on the track itself rather than parked on a separate slider that has to
// explain what it does.
public static class TargetGauge {
    private const float NotchOverhang = 4f;

    private static string? draggingGauge;

    // Returns the target, moved if the player dragged it, so the caller writes it
    // back to whatever field owns it.
    public static float Draw(
        Rect rect,
        float current,
        float max,
        float target,
        GaugePalette palette,
        string dragId
    ) {
        Widgets.DrawBoxSolid(rect, palette.Track);

        if (max > 0f && current > 0f) {
            Widgets.DrawBoxSolid(
                new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(current / max), rect.height),
                palette.Fill
            );
        }

        float moved = HandleDrag(rect, dragId, max, target);
        DrawNotch(rect, moved, max, palette);

        return moved;
    }

    // A threshold of zero is off rather than low, so the notch goes quiet instead
    // of sitting at the far left looking like a gauge that failed to draw.
    private static void DrawNotch(Rect rect, float target, float max, GaugePalette palette) {
        if (max <= 0f) return;

        float x = rect.x + rect.width * Mathf.Clamp01(target / max);
        Widgets.DrawBoxSolid(
            new Rect(x - 1.5f, rect.y - 2f, 3f, rect.height + 2f + NotchOverhang),
            target > 0f ? palette.Notch : palette.NotchIdle
        );
    }

    private static float HandleDrag(Rect rect, string dragId, float max, float target) {
        Event? e = Event.current;
        if (e == null || max <= 0f) return target;

        if (e.type == EventType.MouseDown && Mouse.IsOver(rect)) {
            draggingGauge = dragId;
            e.Use();
        }

        if (draggingGauge != dragId) return target;

        // MouseDrag only reaches a control that claimed the hot control, which a
        // hand-drawn gauge never does, so follow the button state directly.
        if (!Input.GetMouseButton(0)) {
            draggingGauge = null;
            return target;
        }

        return GaugeMath.SnapTarget((e.mousePosition.x - rect.x) / rect.width, max);
    }
}
