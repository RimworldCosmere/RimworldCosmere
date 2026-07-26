using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Radial;

// Holds textures, which the engine wants resolved on the main thread at startup.
[StaticConstructorOnStartup]
public static class RadialWedgeTex {
    private const int TexSize = 256;
    private static readonly Dictionary<int, Texture2D> cache = new Dictionary<int, Texture2D>();
    private static Texture2D? discCache;
    private static Texture2D? vignetteCache;
    private static Texture2D? backingCache;
    private static readonly Dictionary<int, Texture2D> edgeCache = new Dictionary<int, Texture2D>();
    private static Texture2D? gizmoIconCache;

    /// A small spoked ring standing in for the wheel, drawn rather than shipped
    /// as art so it matches whatever the ring geometry becomes.
    public static Texture2D GizmoIcon() {
        if (gizmoIconCache != null) return gizmoIconCache;

        const int size = 64;
        const int spokes = 8;
        const float outer = 0.46f;
        const float inner = 0.20f;
        const float aa = 1.6f / size;
        float halfSpoke = 0.055f;

        Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
        Color32[] pixels = new Color32[size * size];
        for (int py = 0; py < size; py++) {
            for (int px = 0; px < size; px++) {
                float x = (px + 0.5f) / size - 0.5f;
                float y = (py + 0.5f) / size - 0.5f;
                float r = Mathf.Sqrt(x * x + y * y);

                float ring = Mathf.Clamp01((outer - r) / aa) * Mathf.Clamp01((r - inner) / aa);

                // Carve evenly spaced spokes out of the ring.
                float theta = Mathf.Atan2(x, y);
                float step = Mathf.PI * 2f / spokes;
                float offset = Mathf.Abs(Mathf.Repeat(theta + step * 0.5f, step) - step * 0.5f);
                float gap = Mathf.Clamp01((offset - halfSpoke) / (aa * 2f));

                float hub = Mathf.Clamp01((inner * 0.55f - r) / aa);
                float alpha = Mathf.Max(ring * gap, hub);
                pixels[py * size + px] = new Color32(255, 255, 255, (byte)(255f * alpha));
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply(false, true);
        gizmoIconCache = tex;
        return tex;
    }

    public static Texture2D Backing() {
        if (backingCache != null) return backingCache;

        const float outer = 0.5f;
        const float inner = 0.5f * (RadialLayout.AbilityRingInner / RadialLayout.AbilityRingOuter);
        const float aa = 1.5f / TexSize;

        Texture2D tex = new Texture2D(TexSize, TexSize, TextureFormat.ARGB32, false);
        Color32[] pixels = new Color32[TexSize * TexSize];
        for (int py = 0; py < TexSize; py++) {
            for (int px = 0; px < TexSize; px++) {
                float x = (px + 0.5f) / TexSize - 0.5f;
                float y = (py + 0.5f) / TexSize - 0.5f;
                float r = Mathf.Sqrt(x * x + y * y);
                float alpha = Mathf.Clamp01((outer - r) / aa) * Mathf.Clamp01((r - inner) / aa);
                byte shade = DepthShade(r, inner, outer);
                pixels[py * TexSize + px] = new Color32(shade, shade, shade, (byte)(255f * alpha));
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply(false, true);
        backingCache = tex;
        return tex;
    }

    public static Texture2D Vignette() {
        if (vignetteCache != null) return vignetteCache;

        Texture2D tex = new Texture2D(TexSize, TexSize, TextureFormat.ARGB32, false);
        Color32[] pixels = new Color32[TexSize * TexSize];
        for (int py = 0; py < TexSize; py++) {
            for (int px = 0; px < TexSize; px++) {
                float x = (px + 0.5f) / TexSize - 0.5f;
                float y = (py + 0.5f) / TexSize - 0.5f;
                float dist = Mathf.Sqrt(x * x + y * y);
                float t = Mathf.Clamp01(Mathf.InverseLerp(0.5f, 0.32f, dist));
                float smooth = t * t * (3f - 2f * t);
                pixels[py * TexSize + px] = new Color32(255, 255, 255, (byte)(255f * smooth));
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply(false, true);
        vignetteCache = tex;
        return tex;
    }

    private static byte DepthShade(float radius, float inner, float outer) {
        float t = Mathf.Clamp01(Mathf.InverseLerp(inner, outer, radius));
        return (byte)(255f * Mathf.Lerp(1f, 0.52f, t));
    }

    public static Texture2D InnerEdge(int count) {
        if (edgeCache.TryGetValue(count, out Texture2D cached)) return cached;

        float halfArcRad = 180f / count * Mathf.Deg2Rad;
        const float inner = 0.5f * (RadialLayout.AbilityRingInner / RadialLayout.AbilityRingOuter);
        const float aa = 1.5f / TexSize;
        float band = 3.5f / RadialLayout.AbilityRingOuter * 0.5f;

        Texture2D tex = new Texture2D(TexSize, TexSize, TextureFormat.ARGB32, false);
        Color32[] pixels = new Color32[TexSize * TexSize];
        for (int py = 0; py < TexSize; py++) {
            for (int px = 0; px < TexSize; px++) {
                float x = (px + 0.5f) / TexSize - 0.5f;
                float y = (py + 0.5f) / TexSize - 0.5f;
                float r = Mathf.Sqrt(x * x + y * y);
                float theta = Mathf.Abs(Mathf.Atan2(x, y));
                float radialA = Mathf.Clamp01((r - inner) / aa) * Mathf.Clamp01((inner + band - r) / aa);
                float angularA = Mathf.Clamp01((halfArcRad - theta) / (aa * 2f));
                pixels[py * TexSize + px] = new Color32(255, 255, 255, (byte)(255f * radialA * angularA));
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply(false, true);
        edgeCache[count] = tex;
        return tex;
    }

    public static Texture2D Disc() {
        if (discCache != null) return discCache;

        const float aa = 1.5f / TexSize;
        Texture2D tex = new Texture2D(TexSize, TexSize, TextureFormat.ARGB32, false);
        Color32[] pixels = new Color32[TexSize * TexSize];
        for (int py = 0; py < TexSize; py++) {
            for (int px = 0; px < TexSize; px++) {
                float x = (px + 0.5f) / TexSize - 0.5f;
                float y = (py + 0.5f) / TexSize - 0.5f;
                float dist = Mathf.Sqrt(x * x + y * y);
                byte a = (byte)(255f * Mathf.Clamp01((0.5f - dist) / aa));
                pixels[py * TexSize + px] = new Color32(255, 255, 255, a);
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply(false, true);
        discCache = tex;
        return tex;
    }

    public static Texture2D Get(int count) {
        if (cache.TryGetValue(count, out Texture2D cached)) return cached;

        float arcDeg = 360f / count;
        float halfArcRad = arcDeg * 0.5f * Mathf.Deg2Rad;
        const float outer = 0.5f;
        const float inner = 0.5f * (RadialLayout.AbilityRingInner / RadialLayout.AbilityRingOuter);
        const float aa = 1.5f / TexSize;
        const float gapRad = 0f;

        Texture2D tex = new Texture2D(TexSize, TexSize, TextureFormat.ARGB32, false);
        Color32[] pixels = new Color32[TexSize * TexSize];
        for (int py = 0; py < TexSize; py++) {
            for (int px = 0; px < TexSize; px++) {
                float x = (px + 0.5f) / TexSize - 0.5f;
                float y = (py + 0.5f) / TexSize - 0.5f;
                float r = Mathf.Sqrt(x * x + y * y);
                float theta = Mathf.Abs(Mathf.Atan2(x, y));
                float radialA = Mathf.Clamp01((outer - r) / aa) * Mathf.Clamp01((r - inner) / aa);
                float angularA = Mathf.Clamp01((halfArcRad - gapRad - theta) / (aa * 2f));
                byte a = (byte)(255f * radialA * angularA);
                byte shade = DepthShade(r, inner, outer);
                pixels[py * TexSize + px] = new Color32(shade, shade, shade, a);
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply(false, true);
        cache[count] = tex;
        return tex;
    }
}
