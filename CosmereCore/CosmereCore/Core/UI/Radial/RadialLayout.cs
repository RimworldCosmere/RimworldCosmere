using UnityEngine;

namespace Cosmere.Core.UI.Radial;

public static class RadialLayout {
    public const float SystemRingInner = 124f;
    public const float SystemRingOuter = 300f;
    public const float SubsectionRingInner = 124f;
    public const float SubsectionRingOuter = 300f;
    public const float AbilityRingInner = 124f;
    public const float AbilityRingOuter = 300f;
    public const float CenterRadius = 124f;

    public const float IconBandRadius = 180f;
    public const float LabelBandRadius = 266f;

    public static float ChordWidthAt(float radius, int wedgeCount) {
        if (wedgeCount <= 0) return 0f;
        float halfArcRad = 180f / wedgeCount * Mathf.Deg2Rad;
        return 2f * radius * Mathf.Sin(halfArcRad);
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