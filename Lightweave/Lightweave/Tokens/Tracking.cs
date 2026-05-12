using System;

namespace Cosmere.Lightweave.Tokens;

public readonly struct Tracking : IEquatable<Tracking> {
    public float Em { get; }

    private Tracking(float em) {
        Em = em;
    }

    public static Tracking Of(float em) {
        return new Tracking(em);
    }

    public float ToPixels(float fontSizePx) {
        return Em * fontSizePx;
    }

    public static readonly Tracking Tighter = new Tracking(-0.05f);
    public static readonly Tracking Tight = new Tracking(-0.025f);
    public static readonly Tracking Normal = new Tracking(0f);
    public static readonly Tracking Wide = new Tracking(0.025f);
    public static readonly Tracking Wider = new Tracking(0.05f);
    public static readonly Tracking Widest = new Tracking(0.1f);

    public bool Equals(Tracking other) {
        return Em.Equals(other.Em);
    }

    public override bool Equals(object? obj) {
        return obj is Tracking other && Equals(other);
    }

    public override int GetHashCode() {
        return Em.GetHashCode();
    }

    public static bool operator ==(Tracking left, Tracking right) {
        return left.Equals(right);
    }

    public static bool operator !=(Tracking left, Tracking right) {
        return !left.Equals(right);
    }
}
