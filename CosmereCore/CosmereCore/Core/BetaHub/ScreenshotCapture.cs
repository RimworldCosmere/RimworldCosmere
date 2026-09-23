using System;
using UnityEngine;
using Verse;

namespace Cosmere.Core.BetaHub;

/// <summary>
///     Grabs the current frame for a bug report.
/// </summary>
/// <remarks>
///     Deferred to the next repaint because ReadPixels only returns real pixels during one,
///     and because the dialog must not be in the shot.
/// </remarks>
public static class ScreenshotCapture {
    private const int JpegQuality = 85;

    private static Action<byte[]?>? waiting;

    public static void RequestCapture(Action<byte[]?> onCaptured) {
        waiting = onCaptured;
    }

    public static void TryCaptureNow() {
        if (waiting == null) return;
        if (Event.current == null || Event.current.type != EventType.Repaint) return;

        Action<byte[]?> callback = waiting;
        waiting = null;

        byte[]? jpeg = null;
        Texture2D? texture = null;

        try {
            texture = new Texture2D(Verse.UI.screenWidth, Verse.UI.screenHeight, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, Verse.UI.screenWidth, Verse.UI.screenHeight), 0, 0);
            texture.Apply();
            jpeg = texture.EncodeToJPG(JpegQuality);
        } catch (Exception ex) {
            Log.Warn($"Screenshot capture failed: {ex.Message}");
        } finally {
            if (texture != null) UnityEngine.Object.Destroy(texture);
        }

        callback(jpeg);
    }
}
