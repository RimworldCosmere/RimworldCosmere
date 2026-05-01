using UnityEngine;

namespace Cosmere.Lightweave.Overlay;

public static class OverlayAnchor {
    public static Rect CaptureAbsolute(Rect anchor) {
        Vector2 topLeft = GUIUtility.GUIToScreenPoint(new Vector2(anchor.x, anchor.y));
        return new Rect(topLeft.x, topLeft.y, anchor.width, anchor.height);
    }

    public static Rect ResolveLocal(Rect absolute) {
        Vector2 local = GUIUtility.ScreenToGUIPoint(new Vector2(absolute.x, absolute.y));
        return new Rect(local.x, local.y, absolute.width, absolute.height);
    }
}
