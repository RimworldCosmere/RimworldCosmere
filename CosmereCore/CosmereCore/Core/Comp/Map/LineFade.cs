using System;

namespace Cosmere.Core.Comp.Map;

public static class LineFade {
    private const float MinAlpha = 0.3f;
    private const float NearRadius = 3f;

    // Opaque within 3 tiles, then linear down to a 0.3 floor at the edge so distant lines stay
    // visible. A radius of 3 or less is entirely "near", and that guard is what keeps the
    // falloff divide from handing a NaN alpha to the material pool.
    public static float For(float radius, float distance) {
        float falloff = radius - NearRadius;
        if (falloff <= 0f) return 1f;

        float ratio = (radius - Math.Max(distance, NearRadius)) / falloff;
        if (ratio > 1f) return 1f;

        return ratio < MinAlpha ? MinAlpha : ratio;
    }
}
