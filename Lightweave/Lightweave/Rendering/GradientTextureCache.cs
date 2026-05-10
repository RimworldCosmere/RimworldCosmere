using System.Collections.Generic;
using UnityEngine;

namespace Cosmere.Lightweave.Rendering;

public static class GradientTextureCache {
    private const int GradientHeight = 32;
    private static readonly Dictionary<long, Texture2D> Cache = new Dictionary<long, Texture2D>();

    public static Texture2D Vertical(Color top, Color bottom) {
        long key = HashColor(top) * 397L ^ HashColor(bottom);
        if (Cache.TryGetValue(key, out Texture2D existing) && existing != null) {
            return existing;
        }

        Texture2D tex = new Texture2D(1, GradientHeight, TextureFormat.RGBA32, false, false) {
            name = "LightweaveGradient",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave,
        };
        Color[] pixels = new Color[GradientHeight];
        for (int i = 0; i < GradientHeight; i++) {
            float t = i / (float)(GradientHeight - 1);
            pixels[GradientHeight - 1 - i] = Color.Lerp(top, bottom, t);
        }
        tex.SetPixels(pixels);
        tex.Apply(updateMipmaps: false, makeNoLongerReadable: true);
        Cache[key] = tex;
        return tex;
    }

    private static long HashColor(Color c) {
        long r = (long)Mathf.Round(c.r * 1023f) & 0x3FF;
        long g = (long)Mathf.Round(c.g * 1023f) & 0x3FF;
        long b = (long)Mathf.Round(c.b * 1023f) & 0x3FF;
        long a = (long)Mathf.Round(c.a * 1023f) & 0x3FF;
        return (r << 30) | (g << 20) | (b << 10) | a;
    }
}
