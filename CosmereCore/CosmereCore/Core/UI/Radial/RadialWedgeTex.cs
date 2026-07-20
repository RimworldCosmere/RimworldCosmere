using System.Collections.Generic;
using UnityEngine;

namespace Cosmere.Core.UI.Radial;

public static class RadialWedgeTex {
    private const int TexSize = 256;
    private static readonly Dictionary<int, Texture2D> cache = new Dictionary<int, Texture2D>();

    public static Texture2D Get(int count) {
        if (cache.TryGetValue(count, out Texture2D cached)) return cached;

        float arcDeg = 360f / count;
        float halfArcRad = arcDeg * 0.5f * Mathf.Deg2Rad;
        const float outer = 0.5f;
        const float inner = 0.5f * (100f / 190f);
        const float aa = 1.5f / TexSize;
        const float gapRad = 0.012f;

        Texture2D tex = new Texture2D(TexSize, TexSize, TextureFormat.ARGB32, false);
        Color32[] pixels = new Color32[TexSize * TexSize];
        for (int py = 0; py < TexSize; py++) {
            for (int px = 0; px < TexSize; px++) {
                float x = (px + 0.5f) / TexSize - 0.5f;
                float y = (py + 0.5f) / TexSize - 0.5f;
                float r = Mathf.Sqrt(x * x + y * y);
                float theta = Mathf.Abs(Mathf.Atan2(x, -y));
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
