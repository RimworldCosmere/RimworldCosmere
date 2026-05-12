using System.Collections.Generic;
using UnityEngine;

namespace Cosmere.Lightweave.Rendering;

public enum VignetteShape {
    Radial,
    Frame,
    Linear,
}

public enum VignetteEdge {
    Top,
    Bottom,
    Left,
    Right,
}

public static class VignetteTextureCache {
    private const int Size = 256;
    private static readonly Dictionary<long, Texture2D> Cache = new Dictionary<long, Texture2D>();

    public static Texture2D Radial(float falloff = 1.6f) {
        long key = ((long)Mathf.RoundToInt(falloff * 1000f) << 8) | 0x01L;
        if (Cache.TryGetValue(key, out Texture2D existing) && existing != null) {
            return existing;
        }
        Texture2D tex = NewTex("LightweaveVignetteRadial", Size, Size);
        Color[] px = new Color[Size * Size];
        float cx = (Size - 1) * 0.5f;
        float cy = (Size - 1) * 0.5f;
        float maxDist = Mathf.Sqrt(cx * cx + cy * cy);
        for (int y = 0; y < Size; y++) {
            for (int x = 0; x < Size; x++) {
                float dx = x - cx;
                float dy = y - cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy) / maxDist;
                float a = Mathf.Pow(Mathf.Clamp01(d), falloff);
                px[y * Size + x] = new Color(1f, 1f, 1f, a);
            }
        }
        Finalize(tex, px);
        Cache[key] = tex;
        return tex;
    }

    public static Texture2D Frame(float innerSize = 0.62f, float softness = 0.18f) {
        long key = ((long)Mathf.RoundToInt(innerSize * 1000f) << 24)
                   | ((long)Mathf.RoundToInt(softness * 1000f) << 8)
                   | 0x02L;
        if (Cache.TryGetValue(key, out Texture2D existing) && existing != null) {
            return existing;
        }
        Texture2D tex = NewTex("LightweaveVignetteFrame", Size, Size);
        Color[] px = new Color[Size * Size];
        float half = (Size - 1) * 0.5f;
        float lo = Mathf.Clamp01(innerSize - softness);
        float hi = Mathf.Clamp01(innerSize + softness);
        for (int y = 0; y < Size; y++) {
            for (int x = 0; x < Size; x++) {
                float nx = Mathf.Abs(x - half) / half;
                float ny = Mathf.Abs(y - half) / half;
                float n = Mathf.Max(nx, ny);
                float a = Mathf.SmoothStep(lo, hi, n);
                px[y * Size + x] = new Color(1f, 1f, 1f, a);
            }
        }
        Finalize(tex, px);
        Cache[key] = tex;
        return tex;
    }

    public static Texture2D Linear(VignetteEdge edge, float falloff = 1.2f) {
        long key = ((long)(int)edge << 24)
                   | ((long)Mathf.RoundToInt(falloff * 1000f) << 8)
                   | 0x03L;
        if (Cache.TryGetValue(key, out Texture2D existing) && existing != null) {
            return existing;
        }
        bool vertical = edge == VignetteEdge.Top || edge == VignetteEdge.Bottom;
        int w = vertical ? 1 : Size;
        int h = vertical ? Size : 1;
        Texture2D tex = NewTex($"LightweaveVignetteLinear_{edge}", w, h);
        Color[] px = new Color[w * h];
        for (int i = 0; i < Size; i++) {
            float t = i / (float)(Size - 1);
            float ramp = Mathf.Pow(t, falloff);
            float a;
            int idx;
            switch (edge) {
                case VignetteEdge.Top:
                    a = ramp;
                    idx = i;
                    break;
                case VignetteEdge.Bottom:
                    a = ramp;
                    idx = Size - 1 - i;
                    break;
                case VignetteEdge.Left:
                    a = 1f - ramp;
                    idx = i;
                    break;
                case VignetteEdge.Right:
                    a = ramp;
                    idx = i;
                    break;
                default:
                    a = 0f;
                    idx = i;
                    break;
            }
            px[idx] = new Color(1f, 1f, 1f, a);
        }
        Finalize(tex, px);
        Cache[key] = tex;
        return tex;
    }

    private static Texture2D NewTex(string name, int w, int h) {
        return new Texture2D(w, h, TextureFormat.RGBA32, false, false) {
            name = name,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave,
        };
    }

    private static void Finalize(Texture2D tex, Color[] px) {
        tex.SetPixels(px);
        tex.Apply(updateMipmaps: false, makeNoLongerReadable: true);
    }
}
