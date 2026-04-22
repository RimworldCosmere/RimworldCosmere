using RemValue = Cosmere.Core.UI.Lightweave.Types.Rem;

namespace Cosmere.Core.UI.Lightweave.Types;

public abstract record Size
{
    public sealed record Rem(RemValue Value) : Size;
    public sealed record Px(float Value) : Size;
    public sealed record Pct(float Fraction) : Size;
    public sealed record Content : Size;
    public sealed record Flex(int Weight) : Size;

    public static Size OfRem(float v) => new Rem(new RemValue(v));
    public static Size OfPx(float v) => new Px(v);
    public static Size OfPct(float v) => new Pct(v);
    public static readonly Size ContentSize = new Content();
    public static Size OfFlex(int w = 1) => new Flex(w);

    public static implicit operator Size(RemValue r) => new Rem(r);
    public static implicit operator Size(float v) => new Rem(new RemValue(v));
}
