using System.Collections.Generic;
using UnityEngine;

namespace Cosmere.Lightweave.Runtime;

/// Per-frame registry of interactive rects for the currently-rendering Lightweave root.
/// Populated during Paint by any primitive that responds to input; consumed by
/// <see cref="LightweaveWindow"/> to suppress the Move cursor over controls.
internal static class LightweaveHitTracker {
    private static readonly List<Rect> rects = new List<Rect>();

    public static void Clear() {
        rects.Clear();
    }

    public static void Track(Rect rect) {
        rects.Add(rect);
    }

    public static bool IsOver(Vector2 mouse) {
        for (int i = 0; i < rects.Count; i++) {
            if (rects[i].Contains(mouse)) {
                return true;
            }
        }

        return false;
    }
}
