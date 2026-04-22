using UnityEngine;

namespace Cosmere.Core.UI.Lightweave.Types;

public abstract record BackgroundSpec
{
    public sealed record Solid(ColorRef Color) : BackgroundSpec;
    public sealed record Textured(Texture2D Texture, ScaleMode Mode = ScaleMode.StretchToFill, ColorRef? Tint = null) : BackgroundSpec;
    public sealed record Gradient(Texture2D GradientTex, ColorRef? Tint = null) : BackgroundSpec;

    public static BackgroundSpec Of(Color c) => new Solid(c);
    public static BackgroundSpec Of(Tokens.ThemeSlot slot) => new Solid(slot);

    public static implicit operator BackgroundSpec(Color c) => new Solid(c);
    public static implicit operator BackgroundSpec(Tokens.ThemeSlot s) => new Solid(s);
}
