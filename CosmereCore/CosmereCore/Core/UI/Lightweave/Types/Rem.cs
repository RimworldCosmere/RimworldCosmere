namespace Cosmere.Core.UI.Lightweave.Types;

public readonly record struct Rem(float Value)
{
    public float ToPixels() => Value * Spacing.BaseUnit;
    public static implicit operator Rem(float v) => new Rem(v);
    public static Rem operator *(Rem r, float m) => new Rem(r.Value * m);
    public static Rem operator +(Rem a, Rem b) => new Rem(a.Value + b.Value);
}

public static class RemExtensions
{
    public static Rem rem(this float v) => new Rem(v);
    public static Rem rem(this int v) => new Rem(v);
}
