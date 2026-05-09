using UnityEngine;

namespace Cosmere.Lightweave.Types;

public readonly record struct RadiusSpec(
    Rem? TopLeft = null,
    Rem? TopRight = null,
    Rem? BottomRight = null,
    Rem? BottomLeft = null,
    Rem? TopStart = null,
    Rem? TopEnd = null,
    Rem? BottomStart = null,
    Rem? BottomEnd = null
) {
    public static readonly RadiusSpec None = new RadiusSpec();

    public static RadiusSpec All(Rem v) {
        return new RadiusSpec(v, v, v, v);
    }

    public static RadiusSpec Top(Rem v) {
        return new RadiusSpec(v, v);
    }

    public static RadiusSpec Bottom(Rem v) {
        return new RadiusSpec(BottomLeft: v, BottomRight: v);
    }

    public static RadiusSpec StartSide(Rem v) {
        return new RadiusSpec(TopStart: v, BottomStart: v);
    }

    public static RadiusSpec All(Cosmere.Lightweave.Tokens.RadiusScale scale) {
        return All(ResolveRem(scale));
    }

    public static RadiusSpec Top(Cosmere.Lightweave.Tokens.RadiusScale scale) {
        return Top(ResolveRem(scale));
    }

    public static RadiusSpec Bottom(Cosmere.Lightweave.Tokens.RadiusScale scale) {
        return Bottom(ResolveRem(scale));
    }

    public static RadiusSpec StartSide(Cosmere.Lightweave.Tokens.RadiusScale scale) {
        return StartSide(ResolveRem(scale));
    }

    public static Rem ResolveRem(Cosmere.Lightweave.Tokens.RadiusScale scale) {
        Cosmere.Lightweave.Runtime.RenderContext? ctx = Cosmere.Lightweave.Runtime.RenderContext.CurrentOrNull;
        if (ctx == null || ctx.ThemeStack.Count == 0) {
            return new Rem(0f);
        }
        float px = ctx.Theme.GetRadius(scale);
        return new Rem(px / Cosmere.Lightweave.Tokens.Spacing.BaseUnit);
    }

    public Vector4 ResolveVector(Direction dir) {
        float tl = TopLeft?.ToPixels() ?? (dir == Direction.Ltr ? TopStart?.ToPixels() : TopEnd?.ToPixels()) ?? 0f;
        float tr = TopRight?.ToPixels() ?? (dir == Direction.Ltr ? TopEnd?.ToPixels() : TopStart?.ToPixels()) ?? 0f;
        float br = BottomRight?.ToPixels() ??
                   (dir == Direction.Ltr ? BottomEnd?.ToPixels() : BottomStart?.ToPixels()) ?? 0f;
        float bl = BottomLeft?.ToPixels() ??
                   (dir == Direction.Ltr ? BottomStart?.ToPixels() : BottomEnd?.ToPixels()) ?? 0f;
        return new Vector4(tl, tr, br, bl);
    }
}