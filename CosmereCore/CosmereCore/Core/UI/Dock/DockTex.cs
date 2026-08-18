using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Dock;

/// <summary>
///     Nine-slice sheets for rounded panels, generated rather than shipped as art - IMGUI has no
///     rounded-rect primitive. DrawAtlas reads the corner as atlas.width * 0.25, so this 24px sheet yields a 6px corner.
/// </summary>
[StaticConstructorOnStartup]
public static class DockTex {
    public const int AtlasSize = 24;
    public const float Radius = 6f;

    // Solid inside the rounded outline. Tint it to colour a panel.
    public static readonly Texture2D RoundFill = Build(false);

    /// <summary>
    ///     The outline alone, one pixel wide. Drawn over the fill in its own colour so a panel's
    ///     body and its edge stay independently tintable.
    /// </summary>
    public static readonly Texture2D RoundBorder = Build(true);

    private static Texture2D Build(bool strokeOnly) {
        Texture2D tex = new Texture2D(AtlasSize, AtlasSize, TextureFormat.ARGB32, false) {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
        };

        Color[] pixels = new Color[AtlasSize * AtlasSize];

        for (int y = 0; y < AtlasSize; y++) {
            for (int x = 0; x < AtlasSize; x++) {
                float distance = RoundedDistance(x + 0.5f, y + 0.5f);

                // Inside is negative - half a pixel of feather keeps the corner from stair-stepping when stretched.
                float alpha = strokeOnly
                    ? Mathf.Clamp01(1f - Mathf.Abs(distance + 0.5f))
                    : Mathf.Clamp01(0.5f - distance);

                pixels[y * AtlasSize + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return tex;
    }

    // Signed distance to a rounded rectangle filling the sheet. Negative inside.
    private static float RoundedDistance(float x, float y) {
        const float half = AtlasSize / 2f;

        float dx = Mathf.Abs(x - half) - (half - Radius);
        float dy = Mathf.Abs(y - half) - (half - Radius);

        float outside = Mathf.Sqrt(Mathf.Max(dx, 0f) * Mathf.Max(dx, 0f) + Mathf.Max(dy, 0f) * Mathf.Max(dy, 0f));

        return outside + Mathf.Min(Mathf.Max(dx, dy), 0f) - Radius;
    }
}
