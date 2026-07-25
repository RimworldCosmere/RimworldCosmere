using System;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

/// Marks for the collapsed dock rail, built as white alpha masks so the rail can
/// tint them with each system's accent. Kept to bold strokes because they render
/// around twenty pixels across, where fine detail turns to mush.
[StaticConstructorOnStartup]
public static class ScadrialSigilTex {
    private const int Size = 64;

    /// A ringed table with a burning core.
    public static readonly Texture2D Allomancy = Build((r, angle) => {
        float spoke = 0f;
        if (r > 0.38f && r < 0.72f) {
            float offset = Mathf.Abs(Mathf.Repeat(angle + Mathf.PI / 4f, Mathf.PI / 2f) - Mathf.PI / 4f);
            spoke = Mathf.InverseLerp(0.17f, 0.09f, offset);
        }

        return Mathf.Max(Ring(r, 0.82f, 0.13f), Mathf.Max(Disc(r, 0.29f), spoke));
    });

    /// Nested rings: charge held in layers, hollow until filled.
    public static readonly Texture2D Feruchemy = Build((r, _) =>
        Mathf.Max(Ring(r, 0.84f, 0.12f), Ring(r, 0.45f, 0.12f))
    );

    private static Texture2D Build(Func<float, float, float> shape) {
        Texture2D tex = new Texture2D(Size, Size, TextureFormat.ARGB32, false) {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
        };

        for (int y = 0; y < Size; y++) {
            for (int x = 0; x < Size; x++) {
                float nx = (x + 0.5f) / Size * 2f - 1f;
                float ny = (y + 0.5f) / Size * 2f - 1f;
                float r = Mathf.Sqrt(nx * nx + ny * ny);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(shape(r, Mathf.Atan2(ny, nx)))));
            }
        }

        tex.Apply();
        return tex;
    }

    private static float Ring(float r, float radius, float thickness) {
        return Mathf.InverseLerp(thickness, thickness * 0.5f, Mathf.Abs(r - radius));
    }

    private static float Disc(float r, float radius) {
        return Mathf.InverseLerp(radius, radius * 0.78f, r);
    }
}
