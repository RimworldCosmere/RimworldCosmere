using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Dock;

// Nine-slice sheets for rounded panels, built rather than shipped. IMGUI has no
// rounded-rect primitive and Widgets.DrawBoxSolid draws squares, so a corner has to
// come from a texture. Generating it keeps the radius, the stroke and the antialiasing
// in code next to the surfaces that use them, and costs no art asset or bundle rebuild.
//
// Widgets.DrawAtlas takes the corner size as atlas.width * 0.25, so a 24px sheet
// yields a 6px corner - slight, which is what these surfaces want.
[StaticConstructorOnStartup]
public static class DockTex {
    public const int AtlasSize = 24;
    public const float Radius = 6f;

    // Solid inside the rounded outline. Tint it to colour a panel.
    public static readonly Texture2D RoundFill = Build(false);

    // The outline alone, one pixel wide. Drawn over the fill in its own colour so a
    // panel's body and its edge stay independently tintable.
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

                // Inside is negative. Half a pixel of feather either way is what keeps
                // the corner from stair-stepping once the atlas is stretched.
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
