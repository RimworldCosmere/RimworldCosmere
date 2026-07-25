using System.Collections.Generic;
using UnityEngine;

namespace Cosmere.Core.UI.Radial;

public static class RadialWedgeTex {
    private const int TexSize = 256;
    private static readonly Dictionary<int, Texture2D> cache = new Dictionary<int, Texture2D>();
    private static Texture2D? discCache;
    private static Texture2D? vignetteCache;
    private static Texture2D? backingCache;

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
                pixels[py * TexSize + px] = new Color32(255, 255, 255, (byte)(255f * alpha));
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
                pixels[py * TexSize + px] = new Color32(255, 255, 255, a);
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply(false, true);
        cache[count] = tex;
        return tex;
    }
}
