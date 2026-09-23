using UnityEngine;

namespace Cosmere.System.Roshar.Comp.Map;

public static class StormlightOverlayDrawHandler {
    private static int lastDrawFrame;

    public static bool ShouldDraw => lastDrawFrame + 1 >= Time.frameCount;

    public static void DrawThisFrame() {
        lastDrawFrame = Time.frameCount;
    }
}
