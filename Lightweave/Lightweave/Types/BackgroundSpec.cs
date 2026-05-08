using System;
using Cosmere.Lightweave.Tokens;
using UnityEngine;

namespace Cosmere.Lightweave.Types;

public abstract record BackgroundSpec {
    private static readonly Solid[] SolidByThemeSlot = BuildSolidCache();

    private static Solid[] BuildSolidCache() {
        Array values = Enum.GetValues(typeof(ThemeSlot));
        int max = 0;
        foreach (object v in values) {
            int idx = (int)v;
            if (idx > max) {
                max = idx;
            }
        }

        Solid[] cache = new Solid[max + 1];
        foreach (object v in values) {
            int idx = (int)v;
            cache[idx] = new Solid((ThemeSlot)v);
        }

        return cache;
    }

    public static BackgroundSpec Of(Color c) {
        return new Solid(c);
    }

    public static BackgroundSpec Of(ThemeSlot slot) {
        return SolidByThemeSlot[(int)slot];
    }

    public static implicit operator BackgroundSpec(Color c) {
        return new Solid(c);
    }

    public static implicit operator BackgroundSpec(ThemeSlot s) {
        return SolidByThemeSlot[(int)s];
    }

    public sealed record Solid(ColorRef Color) : BackgroundSpec;

    public sealed record Textured(Texture2D Texture, ScaleMode Mode = ScaleMode.StretchToFill, ColorRef? Tint = null)
        : BackgroundSpec;

    public sealed record Gradient(Texture2D GradientTex, ColorRef? Tint = null) : BackgroundSpec;
}