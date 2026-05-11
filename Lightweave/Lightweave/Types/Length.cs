using Cosmere.Lightweave.Tokens;

namespace Cosmere.Lightweave.Types;

public readonly record struct Length {
    public enum Kind {
        Auto,
        Rem,
        Percent,
        Stretch,
        Grow
    }

    public Kind Mode { get; init; }

    public float Value { get; init; }

    public static Length Auto => new() { Mode = Kind.Auto };

    public static Length Stretch => new() { Mode = Kind.Stretch, Value = 1f };

    public static Length Grow(float factor) {
        return new Length { Mode = Kind.Grow, Value = factor };
    }

    public static Length Rem(float remValue) {
        return new Length { Mode = Kind.Rem, Value = remValue };
    }

    public static Length Percent(float percentValue) {
        return new Length { Mode = Kind.Percent, Value = percentValue };
    }

    public bool IsAuto => Mode == Kind.Auto;

    public bool IsGrower => Mode == Kind.Stretch || Mode == Kind.Grow;

    public float GrowFactor => Mode switch {
        Kind.Stretch => 1f,
        Kind.Grow => Value,
        _ => 0f,
    };

    public float ToPixels(float parentSize, float intrinsicSize) {
        return Mode switch {
            Kind.Rem => Value * Spacing.BaseUnit,
            Kind.Percent => parentSize * Value / 100f,
            Kind.Auto => intrinsicSize,
            Kind.Stretch => intrinsicSize,
            Kind.Grow => intrinsicSize,
            _ => intrinsicSize,
        };
    }

    public static implicit operator Length(Rem r) {
        return new Length { Mode = Kind.Rem, Value = r.Value };
    }
}
