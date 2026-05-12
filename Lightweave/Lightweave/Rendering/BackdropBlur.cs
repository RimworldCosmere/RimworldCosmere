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

        Color color = tint ?? Color.white;
        mat.SetFloat(BlurSizeId, blurSizePx);
        mat.SetColor(ColorId, color);

        Graphics.DrawTexture(
            rect,
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

    private static readonly int BlurSizeId = Shader.PropertyToID("_BlurSize");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
}
