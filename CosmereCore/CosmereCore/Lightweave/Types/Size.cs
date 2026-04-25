namespace Cosmere.Lightweave.Types;

public abstract record Size {
    public static readonly Size OfContent = new Content();

    public static Size OfRem(float v) {
        return new Length(new Rem(v));
    }

    public static Size OfPx(float v) {
        return new Px(v);
    }

    public static Size OfPct(float v) {
        return new Pct(v);
    }

    public static Size OfFlex(int w = 1) {
        return new Flex(w);
    }

    public static implicit operator Size(Rem r) {
        return new Length(r);
    }

    public static implicit operator Size(float v) {
        return new Length(new Rem(v));
    }

    public sealed record Length(Rem Value) : Size;

    public sealed record Px(float Value) : Size;

    public sealed record Pct(float Fraction) : Size;

    public sealed record Content : Size;

    public sealed record Flex(int Weight) : Size;
}