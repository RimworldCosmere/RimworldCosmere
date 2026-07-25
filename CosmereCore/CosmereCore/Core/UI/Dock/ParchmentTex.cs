using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Dock;

/// Dark parchment, built rather than shipped so it can be stretched or tiled to
/// any surface without a nine-slice. Mottled by layered value noise, streaked
/// along the grain, and darkened towards the edges the way a handled sheet ages.
[StaticConstructorOnStartup]
public static class ParchmentTex {
    // Declared before the sheets on purpose: static fields initialise in order,
    // and building against a default Color painted them black.
    private static readonly Color Base = new Color(0.180f, 0.149f, 0.114f);

    private const int SheetWidth = 192;
    private const int SheetHeight = 96;

    /// Large enough that a panel rarely shows the same patch twice. Tiling a small
    /// sheet across a tall window repeated often enough to read as a pattern.
    private const int FieldSize = 512;

    /// A single sheet, aged at its edges. For surfaces drawn one to one.
    public static readonly Texture2D Sheet = Build(SheetWidth, SheetHeight, true);

    /// The same stock without the aged rim, so it tiles across a large panel
    /// without the darkened edges repeating as a grid of seams.
    public static readonly Texture2D Field = Build(FieldSize, FieldSize, false);

    /// Tiles the field across a rect at its natural scale, so the grain does not
    /// stretch with the panel.
    public static void DrawField(Rect rect) {
        GUI.DrawTextureWithTexCoords(
            rect,
            Field,
            new Rect(0f, 0f, rect.width / FieldSize, rect.height / FieldSize)
        );
    }

    private static Texture2D Build(int width, int height, bool aged) {
        Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false) {
            wrapMode = aged ? TextureWrapMode.Clamp : TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear,
        };

        // Written in one go: SetPixel per pixel is far slower, and the field is
        // large enough for that to show at startup.
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                float u = (x + 0.5f) / width;
                float v = (y + 0.5f) / height;

                // Broad shape first, then successively finer tooth. The field
                // covers a panel about once over, so the fine octaves read as
                // surface rather than as a repeating pattern.
                float mottle = Tiled(u, v, 2, 2, 1) * 0.38f
                               + Tiled(u, v, 5, 5, 2) * 0.24f
                               + Tiled(u, v, 11, 11, 3) * 0.18f
                               + Tiled(u, v, 23, 23, 4) * 0.12f
                               + Tiled(u, v, 47, 47, 5) * 0.08f;

                // Fibres run the long way, so the grain is stretched across x.
                float fibre = Tiled(u, v, 4, 64, 6);

                // Occasional darker patches, as though the sheet has been handled
                // or spotted. Thresholded so most of it stays clean.
                float stain = Mathf.InverseLerp(0.56f, 0.88f, Tiled(u, v, 3, 3, 7));

                float shade = 1f
                              - (mottle - 0.5f) * 0.46f
                              - (fibre - 0.5f) * 0.17f
                              - stain * 0.20f
                              - (aged ? EdgeFalloff(u, v) * 0.30f : 0f);

                Color c = Base * shade;
                pixels[y * width + x] = new Color(c.r, c.g, c.b, 1f);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return tex;
    }

    /// Darkens towards the rim, strongest in the corners.
    private static float EdgeFalloff(float u, float v) {
        float horizontal = 1f - Mathf.Clamp01(Mathf.Min(u, 1f - u) / 0.12f);
        float vertical = 1f - Mathf.Clamp01(Mathf.Min(v, 1f - v) / 0.2f);

        return Mathf.Clamp01(Mathf.Max(horizontal, vertical));
    }

    /// Value noise whose lattice wraps at the given cycle counts, so sampling the
    /// full sheet joins back to itself on both axes. The seed decorrelates layers
    /// that share a frequency.
    private static float Tiled(float u, float v, int cyclesX, int cyclesY, int seed) {
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

        float top = Mathf.Lerp(Hash(x0, y0, seed), Hash(x1, y0, seed), sx);
        float bottom = Mathf.Lerp(Hash(x0, y1, seed), Hash(x1, y1, seed), sx);

        return Mathf.Lerp(top, bottom, sy);
    }

    private static int Wrap(int value, int period) {
        return (value % period + period) % period;
    }

    private static float Hash(int x, int y, int seed) {
        int n = x * 374761393 + y * 668265263 + seed * 1442695041;
        n = (n ^ (n >> 13)) * 1274126177;

        return ((n ^ (n >> 16)) & 0x7FFFFFF) / (float)0x7FFFFFF;
    }
}
