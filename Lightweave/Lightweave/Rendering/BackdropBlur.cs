using System.Reflection;
using UnityEngine;

namespace Cosmere.Lightweave.Rendering;

public static class BackdropBlur {
    public static void Draw(Rect rect, float blurSizePx = 12f, Color? tint = null) {
        Material? mat = LightweaveShaderDatabase.BlurMaterial;
        if (mat == null) {
            return;
        }

        if (Event.current.type != EventType.Repaint) {
            return;
        }

        Rect clipped = ClipToVisibleRect(rect);
        if (clipped.width <= 0f || clipped.height <= 0f) {
            return;
        }

        Color color = tint ?? Color.white;
        mat.SetFloat(BlurSizeId, blurSizePx);
        mat.SetColor(ColorId, color);

        Graphics.DrawTexture(
            clipped,
            Texture2D.whiteTexture,
            new Rect(0f, 0f, 1f, 1f),
            0,
            0,
            0,
            0,
            color,
            mat
        );
    }

    private static Rect ClipToVisibleRect(Rect rect) {
        if (VisibleRectProp == null) {
            return rect;
        }

        object? value;
        try {
            value = VisibleRectProp.GetValue(null);
        }
        catch {
            return rect;
        }

        if (value is not Rect visible) {
            return rect;
        }

        float x0 = Mathf.Max(rect.xMin, visible.xMin);
        float y0 = Mathf.Max(rect.yMin, visible.yMin);
        float x1 = Mathf.Min(rect.xMax, visible.xMax);
        float y1 = Mathf.Min(rect.yMax, visible.yMax);
        return new Rect(x0, y0, x1 - x0, y1 - y0);
    }

    private static readonly int BlurSizeId = Shader.PropertyToID("_BlurSize");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private static readonly PropertyInfo? VisibleRectProp =
        typeof(GUI).Assembly
            .GetType("UnityEngine.GUIClip")
            ?.GetProperty("visibleRect", BindingFlags.Static | BindingFlags.NonPublic);
}
