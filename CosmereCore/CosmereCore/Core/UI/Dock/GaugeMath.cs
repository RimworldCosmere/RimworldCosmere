using System;

namespace Cosmere.Core.UI.Dock;

/// <summary>
///     Where a dragged threshold actually lands. Deliberately free of UnityEngine - the arithmetic
///     never needed a Rect, and staying plain means it can be checked without a game running.
/// </summary>
public static class GaugeMath {
    public const float SnapStep = 0.05f;

    public static float SnapTarget(float fraction, float max) {
        if (max <= 0f) return 0f;

        float clamped = fraction < 0f ? 0f : fraction > 1f ? 1f : fraction;

        return (float)Math.Round(clamped / SnapStep) * SnapStep * max;
    }
}
