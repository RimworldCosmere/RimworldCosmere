using System;
using Cosmere.Lightweave.Tokens;
using UnityEngine;

namespace Cosmere.Lightweave.Types;

public abstract record BackgroundSpec {
    private static readonly BackgroundSpec[] ByThemeSlot = BuildSlotCache();

    private static BackgroundSpec[] BuildSlotCache() {
        Array values = Enum.GetValues(typeof(ThemeSlot));
        int max = 0;
        foreach (object v in values) {
            int idx = (int)v;
            if (idx > max) {
                max = idx;
            }
        }

        BackgroundSpec[] cache = new BackgroundSpec[max + 1];
        foreach (object v in values) {
            ThemeSlot slot = (ThemeSlot)v;
            int idx = (int)slot;
            cache[idx] = slot == ThemeSlot.SurfaceSunken
                ? (BackgroundSpec)new Blurred(slot)
                : new Solid(slot);
        }

        return cache;
    }

    public static BackgroundSpec Of(Color c) {
        return new Solid(c);
    }

    public static BackgroundSpec Of(ThemeSlot slot) {
        return ByThemeSlot[(int)slot];
    }

    public static BackgroundSpec Blur(ColorRef? tint = null, float blurSizePx = 4f) {
        return new Blurred(tint, blurSizePx);
    }

    public static implicit operator BackgroundSpec(Color c) {
        return new Solid(c);
    }

    public static implicit operator BackgroundSpec(ThemeSlot s) {
        return ByThemeSlot[(int)s];
    }

    public sealed record Solid(ColorRef Color) : BackgroundSpec;

    public sealed record Textured(Texture2D Texture, ScaleMode Mode = ScaleMode.StretchToFill, ColorRef? Tint = null)
        : BackgroundSpec;

    public sealed record Gradient(Texture2D GradientTex, ColorRef? Tint = null) : BackgroundSpec;

    public sealed record Blurred(ColorRef? Tint = null, float BlurSizePx = 12f) : BackgroundSpec;
}