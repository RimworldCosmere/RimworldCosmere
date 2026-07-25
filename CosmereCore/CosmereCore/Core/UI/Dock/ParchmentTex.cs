using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Dock;

/// A sheet of dark parchment, built rather than shipped so it can be stretched to
/// any ribbon without a nine-slice. Mottled by layered value noise, streaked
/// along the grain, and darkened towards the edges the way a handled sheet ages.
[StaticConstructorOnStartup]
public static class ParchmentTex {
    private const int Width = 192;
    private const int Height = 96;

    // Declared before Sheet on purpose: static fields initialise in order, and
    // building the sheet against a default Color painted it black.
    private static readonly Color Base = new Color(0.180f, 0.149f, 0.114f);

    public static readonly Texture2D Sheet = Build();

    private static Texture2D Build() {
        Texture2D tex = new Texture2D(Width, Height, TextureFormat.ARGB32, false) {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
        };

        for (int y = 0; y < Height; y++) {
            for (int x = 0; x < Width; x++) {
                float u = (x + 0.5f) / Width;
                float v = (y + 0.5f) / Height;

                // Three octaves of blotching, coarse to fine.
                float mottle = ValueNoise(x * 0.05f, y * 0.05f) * 0.6f
                               + ValueNoise(x * 0.14f, y * 0.14f) * 0.3f
                               + ValueNoise(x * 0.4f, y * 0.4f) * 0.1f;

                // Fibres run the long way, so the grain is stretched across x.
                float fibre = ValueNoise(x * 0.5f, y * 4f);

                // Dark stock takes a heavier hand: the same variation reads as
                // almost nothing once the ground is this deep.
                float shade = 1f
                              - (mottle - 0.5f) * 0.42f
                              - (fibre - 0.5f) * 0.14f
                              - EdgeFalloff(u, v) * 0.30f;

                Color c = Base * shade;
                tex.SetPixel(x, y, new Color(c.r, c.g, c.b, 1f));
            }
        }

        tex.Apply();
        return tex;
    }

    /// Darkens towards the rim, strongest in the corners.
    private static float EdgeFalloff(float u, float v) {
        float horizontal = 1f - Mathf.Clamp01(Mathf.Min(u, 1f - u) / 0.12f);
        float vertical = 1f - Mathf.Clamp01(Mathf.Min(v, 1f - v) / 0.2f);

        return Mathf.Clamp01(Mathf.Max(horizontal, vertical));
    }

    private static float ValueNoise(float x, float y) {
        int xi = Mathf.FloorToInt(x);
        int yi = Mathf.FloorToInt(y);
        float xf = x - xi;
        float yf = y - yi;

        // Smoothstep the interpolant so the lattice does not show as a grid.
        float sx = xf * xf * (3f - 2f * xf);
        float sy = yf * yf * (3f - 2f * yf);

        float top = Mathf.Lerp(Hash(xi, yi), Hash(xi + 1, yi), sx);
        float bottom = Mathf.Lerp(Hash(xi, yi + 1), Hash(xi + 1, yi + 1), sx);

        return Mathf.Lerp(top, bottom, sy);
    }

    private static float Hash(int x, int y) {
        int n = x * 374761393 + y * 668265263;
        n = (n ^ (n >> 13)) * 1274126177;

        return ((n ^ (n >> 16)) & 0x7FFFFFF) / (float)0x7FFFFFF;
    }
}
