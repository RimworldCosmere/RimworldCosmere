using System.Collections.Generic;
using UnityEngine;

namespace Cosmere.Lightweave.Runtime;

internal static class HoverBlockRegistry {
    private static int currentFrame = -1;
    private static readonly List<Rect> rects = new List<Rect>();

    private static void EnsureFrame() {
        int frame = Time.frameCount;
        if (frame != currentFrame) {
            currentFrame = frame;
            rects.Clear();
        }
    }

    public static void Register(Rect screenRect) {
        EnsureFrame();
        rects.Add(screenRect);
    }

    public static bool IsBlocked(Vector2 screenPos) {
        EnsureFrame();
        for (int i = 0; i < rects.Count; i++) {
            if (rects[i].Contains(screenPos)) {
                return true;
            }
        }
        return false;
    }
}
