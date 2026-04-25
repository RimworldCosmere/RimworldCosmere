using UnityEngine;

namespace Cosmere.Lightweave.Types;

public readonly record struct EdgeInsets(
    Rem? Top = null,
    Rem? Right = null,
    Rem? Bottom = null,
    Rem? Left = null,
    Rem? Start = null,
    Rem? End = null
) {
    public static readonly EdgeInsets Zero = new EdgeInsets();

    public static EdgeInsets All(Rem v) {
        return new EdgeInsets(v, v, v, v);
    }

    public static EdgeInsets Horizontal(Rem v) {
        return new EdgeInsets(Left: v, Right: v);
    }

    public static EdgeInsets Vertical(Rem v) {
        return new EdgeInsets(v, Bottom: v);
    }

    public static EdgeInsets FromStart(Rem v) {
        return new EdgeInsets(Start: v);
    }

    public static EdgeInsets FromEnd(Rem v) {
        return new EdgeInsets(End: v);
    }

    public (float Left, float Top, float Right, float Bottom) Resolve(Direction dir) {
        float startPx = Start?.ToPixels() ?? 0f;
        float endPx = End?.ToPixels() ?? 0f;
        float leftPx = (Left?.ToPixels() ?? 0f) + (dir == Direction.Ltr ? startPx : endPx);
        float rightPx = (Right?.ToPixels() ?? 0f) + (dir == Direction.Ltr ? endPx : startPx);
        return (leftPx, Top?.ToPixels() ?? 0f, rightPx, Bottom?.ToPixels() ?? 0f);
    }

    public Rect Shrink(Rect r, Direction dir) {
        (float left, float top, float right, float bottom) = Resolve(dir);
        return new Rect(r.x + left, r.y + top, r.width - left - right, r.height - top - bottom);
    }
}