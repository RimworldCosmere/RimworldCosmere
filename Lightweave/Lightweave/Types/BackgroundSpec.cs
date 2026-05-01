using Cosmere.Lightweave.Tokens;
using UnityEngine;

namespace Cosmere.Lightweave.Types;

public abstract record BackgroundSpec {
    public static BackgroundSpec Of(Color c) {
        return new Solid(c);
    }

    public static BackgroundSpec Of(ThemeSlot slot) {
        return new Solid(slot);
    }

    public static implicit operator BackgroundSpec(Color c) {
        return new Solid(c);
    }

    public static implicit operator BackgroundSpec(ThemeSlot s) {
        return new Solid(s);
    }

    public sealed record Solid(ColorRef Color) : BackgroundSpec;

    public sealed record Textured(Texture2D Texture, ScaleMode Mode = ScaleMode.StretchToFill, ColorRef? Tint = null)
        : BackgroundSpec;

    public sealed record Gradient(Texture2D GradientTex, ColorRef? Tint = null) : BackgroundSpec;
}