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
}