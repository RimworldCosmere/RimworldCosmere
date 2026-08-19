using System;

namespace Cosmere.Core.Comp.Map;

public static class LineFade {
    private const float MinAlpha = 0.3f;
    private const float NearRadius = 3f;

    /// <summary>
    ///     Opaque within 3 tiles, then linear down to a 0.3 floor at the edge. A radius of 3 or
    ///     less is entirely "near" - that guard also stops the falloff divide handing back NaN.
    /// </summary>
    public static float For(float radius, float distance) {
        float falloff = radius - NearRadius;
        if (falloff <= 0f) return 1f;

        float ratio = (radius - Math.Max(distance, NearRadius)) / falloff;
        if (ratio > 1f) return 1f;

        return ratio < MinAlpha ? MinAlpha : ratio;
    }
}
