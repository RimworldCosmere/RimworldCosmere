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

    /// A single sheet, aged at its edges. For anything drawn one-to-one.
    public static readonly Texture2D Sheet = Build(true);

    /// The same stock without the aged rim, so it tiles across a large panel
    /// without the darkened edges showing up as a grid of seams.
    public static readonly Texture2D Field = Build(false);

    /// Tiles the field across a rect at its natural scale, so the grain does not
    /// stretch with the panel.
    public static void DrawField(Rect rect) {
        GUI.DrawTextureWithTexCoords(
            rect,
            Field,
            new Rect(0f, 0f, rect.width / Width, rect.height / Height)
        );
    }

    private static Texture2D Build(bool aged) {
        Texture2D tex = new Texture2D(Width, Height, TextureFormat.ARGB32, false) {
            wrapMode = aged ? TextureWrapMode.Clamp : TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear,
        };

        for (int y = 0; y < Height; y++) {
            for (int x = 0; x < Width; x++) {
                float u = (x + 0.5f) / Width;
                float v = (y + 0.5f) / Height;

                // Frequencies are whole cycles across the sheet so the lattice
                // wraps, which is what lets the field tile without seams.
                float mottle = Tiled(u, v, 6) * 0.6f
                               + Tiled(u, v, 14) * 0.3f
                               + Tiled(u, v, 32) * 0.1f;

                // Fibres run the long way, so the grain is stretched across x.
                float fibre = Tiled(u, v, 8, 48);

                // Dark stock takes a heavier hand: the same variation reads as
                // almost nothing once the ground is this deep.
                float shade = 1f
                              - (mottle - 0.5f) * 0.42f
                              - (fibre - 0.5f) * 0.14f
                              - (aged ? EdgeFalloff(u, v) * 0.30f : 0f);

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

    private static float Tiled(float u, float v, int cycles) {
        return Tiled(u, v, cycles, cycles);
    }

    /// Value noise whose lattice wraps at the given cycle counts, so sampling the
    /// full sheet joins back to itself on both axes.
    private static float Tiled(float u, float v, int cyclesX, int cyclesY) {
        float x = u * cyclesX;
        float y = v * cyclesY;
        int xi = Mathf.FloorToInt(x);
        int yi = Mathf.FloorToInt(y);
        float xf = x - xi;
        float yf = y - yi;

        // Smoothstep the interpolant so the lattice does not show as a grid.
        float sx = xf * xf * (3f - 2f * xf);
        float sy = yf * yf * (3f - 2f * yf);

        int x0 = Wrap(xi, cyclesX);
        int x1 = Wrap(xi + 1, cyclesX);
        int y0 = Wrap(yi, cyclesY);
        int y1 = Wrap(yi + 1, cyclesY);

        float top = Mathf.Lerp(Hash(x0, y0), Hash(x1, y0), sx);
        float bottom = Mathf.Lerp(Hash(x0, y1), Hash(x1, y1), sx);

        return Mathf.Lerp(top, bottom, sy);
    }

    private static int Wrap(int value, int period) {
        return (value % period + period) % period;
    }

    private static float Hash(int x, int y) {
        int n = x * 374761393 + y * 668265263;
        n = (n ^ (n >> 13)) * 1274126177;

        return ((n ^ (n >> 16)) & 0x7FFFFFF) / (float)0x7FFFFFF;
    }
}
