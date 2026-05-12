using System.Collections.Generic;
using UnityEngine;

namespace Cosmere.Lightweave.Rendering;

public static class ChevronTextureCache {
    private const int Size = 256;
    private static readonly Dictionary<long, Texture2D> Cache = new Dictionary<long, Texture2D>();

    public static Texture2D Down(float strokePx = 22f, float armSpan = 0.56f, float armHeight = 0.28f) {
        long key = ((long)Mathf.RoundToInt(strokePx * 100f) << 32)
                   | ((long)Mathf.RoundToInt(armSpan * 1000f) << 16)
                   | ((long)Mathf.RoundToInt(armHeight * 1000f) << 1)
                   | 0x01L;
        if (Cache.TryGetValue(key, out Texture2D existing) && existing != null) {
            return existing;
        }
        Texture2D tex = NewTex("LightweaveChevronDown", Size, Size);
        Color[] px = new Color[Size * Size];

        float cx = (Size - 1) * 0.5f;
        float cy = (Size - 1) * 0.5f;
        float armX = Size * armSpan * 0.5f;
        float armY = Size * armHeight * 0.5f;

        Vector2 apex = new Vector2(cx, cy + armY);
        Vector2 leftTip = new Vector2(cx - armX, cy - armY);
        Vector2 rightTip = new Vector2(cx + armX, cy - armY);

        float halfStroke = strokePx * 0.5f;
        float aaEdge = 1.4f;
        float innerEdge = halfStroke - aaEdge;

        for (int y = 0; y < Size; y++) {
            for (int x = 0; x < Size; x++) {
                Vector2 p = new Vector2(x, y);
                float d1 = DistToSegment(p, leftTip, apex);
                float d2 = DistToSegment(p, rightTip, apex);
                float d = Mathf.Min(d1, d2);
                float a;
                if (d <= innerEdge) {
                    a = 1f;
                }
                else if (d >= halfStroke) {
                    a = 0f;
                }
                else {
                    a = 1f - (d - innerEdge) / aaEdge;
                }
                px[y * Size + x] = new Color(1f, 1f, 1f, a);
            }
        }

        Finalize(tex, px);
        Cache[key] = tex;
        return tex;
    }

    private static float DistToSegment(Vector2 p, Vector2 a, Vector2 b) {
        Vector2 ab = b - a;
        Vector2 ap = p - a;
        float lenSq = ab.sqrMagnitude;
        float t = lenSq <= 0f ? 0f : Mathf.Clamp01(Vector2.Dot(ap, ab) / lenSq);
        Vector2 closest = a + t * ab;
        return Vector2.Distance(p, closest);
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
