using UnityEngine;

namespace Cosmere.Lightweave.Rendering;

public static class RectSnap {
    public static Rect Snap(Rect r) {
        return new Rect(
            Mathf.Round(r.x),
            Mathf.Round(r.y),
            Mathf.Round(r.width),
            Mathf.Round(r.height)
        );
    }

    public static Rect SnapText(Rect r) {
        return new Rect(
            Mathf.Floor(r.x),
            Mathf.Floor(r.y),
            Mathf.Ceil(r.width),
            Mathf.Ceil(r.height)
        );
    }

    public static Vector2 SnapPoint(Vector2 p) {
        return new Vector2(Mathf.Round(p.x), Mathf.Round(p.y));
    }
}