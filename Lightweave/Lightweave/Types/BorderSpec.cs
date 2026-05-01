using UnityEngine;

namespace Cosmere.Lightweave.Types;

public readonly record struct BorderSpec(
    Rem? Top = null,
    Rem? Right = null,
    Rem? Bottom = null,
    Rem? Left = null,
    Rem? Start = null,
    Rem? End = null,
    ColorRef? Color = null
) {
    public static BorderSpec All(Rem width, ColorRef color) {
        return new BorderSpec(width, width, width, width, Color: color);
    }

    public Vector4 ResolveVector(Direction dir) {
        float startPx = Start?.ToPixels() ?? 0f;
        float endPx = End?.ToPixels() ?? 0f;
        float leftPx = (Left?.ToPixels() ?? 0f) + (dir == Direction.Ltr ? startPx : endPx);
        float rightPx = (Right?.ToPixels() ?? 0f) + (dir == Direction.Ltr ? endPx : startPx);
        return new Vector4(leftPx, Top?.ToPixels() ?? 0f, rightPx, Bottom?.ToPixels() ?? 0f);
    }
}