using System.Collections.Generic;
using UnityEngine;

namespace Cosmere.Lightweave.Runtime;

internal static class HoverBlockRegistry {
    private static List<Rect> current = new List<Rect>();
    private static List<Rect> previous = new List<Rect>();

    public static void BeginFrame() {
        List<Rect> temp = previous;
        previous = current;
        current = temp;
        current.Clear();
    }

    public static void Register(Rect screenRect) {
        current.Add(screenRect);
    }

    public static bool IsBlocked(Vector2 screenPos) {
        for (int i = 0; i < current.Count; i++) {
            if (current[i].Contains(screenPos)) {
                return true;
            }
        }
        for (int i = 0; i < previous.Count; i++) {
            if (previous[i].Contains(screenPos)) {
                return true;
            }
        }
        return false;
    }
}
