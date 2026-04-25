using Cosmere.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Lightweave.Overlay;

internal static class PopoverLayout {
    public static Rect Resolve(
        Rect anchor,
        PopoverPlacement placement,
        Direction dir,
        Vector2 size,
        Rect windowBounds
    ) {
        float gap = new Rem(0.25f).ToPixels();
        Rect candidate;
        switch (placement) {
            case PopoverPlacement.Top:
                candidate = new Rect(anchor.x, anchor.y - size.y - gap, size.x, size.y);
                break;
            case PopoverPlacement.Bottom:
                candidate = new Rect(anchor.x, anchor.yMax + gap, size.x, size.y);
                break;
            case PopoverPlacement.Start:
                candidate = dir == Direction.Ltr
                    ? new Rect(anchor.x - size.x - gap, anchor.y, size.x, size.y)
                    : new Rect(anchor.xMax + gap, anchor.y, size.x, size.y);
                break;
            case PopoverPlacement.End:
                candidate = dir == Direction.Ltr
                    ? new Rect(anchor.xMax + gap, anchor.y, size.x, size.y)
                    : new Rect(anchor.x - size.x - gap, anchor.y, size.x, size.y);
                break;
            default:
                candidate = ResolveAuto(anchor, size, windowBounds, gap);
                break;
        }

        float maxX = windowBounds.xMax - size.x;
        float maxY = windowBounds.yMax - size.y;
        if (maxX < windowBounds.x) {
            maxX = windowBounds.x;
        }

        if (maxY < windowBounds.y) {
            maxY = windowBounds.y;
        }

        candidate.x = Mathf.Clamp(candidate.x, windowBounds.x, maxX);
        candidate.y = Mathf.Clamp(candidate.y, windowBounds.y, maxY);
        return candidate;
    }

    private static Rect ResolveAuto(Rect anchor, Vector2 size, Rect windowBounds, float gap) {
        if (anchor.yMax + size.y + gap <= windowBounds.yMax) {
            return new Rect(anchor.x, anchor.yMax + gap, size.x, size.y);
        }

        return new Rect(anchor.x, anchor.y - size.y - gap, size.x, size.y);
    }
}