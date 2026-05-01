using Cosmere.Lightweave.Tokens;

namespace Cosmere.Lightweave.Types;

public readonly record struct Rem(float Value) {
    public float ToPixels() {
        return Value * Spacing.BaseUnit;
    }

    public float ToFontPx() {
        return Value * Spacing.FontBaseUnit;
    }

    public static implicit operator Rem(float v) {
        return new Rem(v);
    }

    public static Rem operator *(Rem r, float m) {
        return new Rem(r.Value * m);
    }

    public static Rem operator *(float m, Rem r) {
        return new Rem(r.Value * m);
    }

    public static Rem operator +(Rem a, Rem b) {
        return new Rem(a.Value + b.Value);
    }
}

public static class RemExtensions {
    public static Rem rem(this float v) {
        return new Rem(v);
    }

    public static Rem rem(this int v) {
        return new Rem(v);
    }
}