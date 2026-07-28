using UnityEngine;

namespace Cosmere.Core.UI.Radial;

public static class RadialLayout {
    public const float SystemRingInner = 140f;
    public const float SystemRingOuter = 340f;
    public const float SubsectionRingInner = 140f;
    public const float SubsectionRingOuter = 340f;
    public const float AbilityRingInner = 140f;
    public const float AbilityRingOuter = 340f;
    public const float CenterRadius = 140f;

    public const float IconBandRadius = 202f;
    public const float LabelBandRadius = 300f;

    public static float ChordWidthAt(float radius, int wedgeCount) {
        if (wedgeCount <= 0) return 0f;
        float halfArcRad = 180f / wedgeCount * Mathf.Deg2Rad;
        return 2f * radius * Mathf.Sin(halfArcRad);
    }

    /// <summary>
    ///     The widest horizontal, axis-aligned label that still fits inside wedge
    ///     <paramref name="index" /> at <paramref name="radius" />.
    /// </summary>
    /// <remarks>
    ///     The chord alone is not enough. It measures the wedge across its own bisector, which is only
    ///     the horizontal width for a wedge sitting straight up or straight down; on a diagonal wedge a
    ///     horizontal box centred on the bisector runs into the edge rays well before it is a chord
    ///     wide, which is how labels ended up spilling over the divider. So the edge rays are
    ///     intersected directly, and the ring's own inner and outer walls bound the near-horizontal
    ///     wedges where the label runs radially instead of across.
    /// </remarks>
    public static float LabelFitWidthAt(int index, int wedgeCount, float radius, float innerRadius, float outerRadius) {
        if (wedgeCount <= 0) return 0f;

        float halfArcDeg = 180f / wedgeCount;
        float midDeg = index * (360f / wedgeCount);
        float midRad = midDeg * Mathf.Deg2Rad;

        float px = Mathf.Sin(midRad) * radius;
        float py = -Mathf.Cos(midRad) * radius;

        float half = ChordWidthAt(radius, wedgeCount) / 2f;
        half = Mathf.Min(half, EdgeRayHalfWidth(px, py, midDeg - halfArcDeg));
        half = Mathf.Min(half, EdgeRayHalfWidth(px, py, midDeg + halfArcDeg));

        // A wedge pointing left or right puts the label along the radius, where the walls bound it.
        float radialRoom = Mathf.Max(Mathf.Abs(Mathf.Sin(midRad)), 0.0001f);
        half = Mathf.Min(half, Mathf.Min(radius - innerRadius, outerRadius - radius) / radialRoom);

        return Mathf.Max(0f, half * 2f);
    }

    public static int HitTest(
        Vector2 mouse,
        Vector2 center,
        float innerRadius,
        float outerRadius,
        int wedgeCount
    ) {
        if (wedgeCount <= 0) return -1;

        Vector2 delta = mouse - center;
        float dist = delta.magnitude;
        if (dist < innerRadius || dist > outerRadius) return -1;

        float angle = Mathf.Atan2(delta.x, -delta.y) * Mathf.Rad2Deg;
        if (angle < 0f) angle += 360f;

        float wedgeSize = 360f / wedgeCount;
        float shifted = angle + wedgeSize / 2f;
        if (shifted >= 360f) shifted -= 360f;

        int index = Mathf.FloorToInt(shifted / wedgeSize);
        if (index < 0) index = 0;
        if (index >= wedgeCount) index = wedgeCount - 1;
        return index;
    }

    // Where a horizontal line through (px, py) meets the ray leaving the centre at edgeDeg. An edge
    // that is itself horizontal never meets it, and is reported as unbounded.
    private static float EdgeRayHalfWidth(float px, float py, float edgeDeg) {
        float rad = edgeDeg * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        if (Mathf.Abs(cos) < 0.0001f) return float.MaxValue;

        float t = (-py * (Mathf.Sin(rad) / cos)) - px;
        return Mathf.Abs(t);
    }

    public static (float startDeg, float endDeg) WedgeAngles(int index, int wedgeCount) {
        float wedgeSize = 360f / wedgeCount;
        float start = index * wedgeSize - wedgeSize / 2f;
        float end = start + wedgeSize;
        return (start, end);
    }

    public static Vector2 WedgeMidpoint(int index, int wedgeCount, float radius, Vector2 center) {
        float wedgeSize = 360f / wedgeCount;
        float midDeg = index * wedgeSize;
        float rad = midDeg * Mathf.Deg2Rad;
        return new Vector2(
            center.x + Mathf.Sin(rad) * radius,
            center.y - Mathf.Cos(rad) * radius
        );
    }
}
