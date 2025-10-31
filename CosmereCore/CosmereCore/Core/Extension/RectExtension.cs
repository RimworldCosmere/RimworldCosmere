using Cosmere.Core.UI;
using UnityEngine;

namespace Cosmere.Core.Extension;

public static class RectExtension {
    public static Rect ContractedBy(this Rect rect, Padding padding) {
        return new Rect(
            rect.x + padding.left,
            rect.y + padding.top,
            rect.width - padding.left - padding.right,
            rect.height - padding.top - padding.bottom
        );
    }

    public static Rect ExpandedBy(this Rect rect, Padding padding) {
        return new Rect(
            rect.x - padding.left,
            rect.y - padding.top,
            rect.width + padding.left + padding.right,
            rect.height + padding.top + padding.bottom
        );
    }

    public static Rect With(
        this Rect rect,
        float? x = null,
        float? y = null,
        float? width = null,
        float? height = null,
        float? paddingX = 0,
        float? paddingY = 0,
        float? padding = 0
    ) {
        Rect newRect = new Rect(rect);
        if (x.HasValue) newRect.x = x.Value;
        if (y.HasValue) newRect.y = y.Value;
        if (width.HasValue) newRect.width = width.Value;
        if (height.HasValue) newRect.height = height.Value;

        if (paddingX.HasValue) newRect = newRect.ContractedBy(new Padding(0, paddingX.Value));
        if (paddingY.HasValue) newRect = newRect.ContractedBy(new Padding(paddingY.Value, 0));
        if (padding.HasValue) newRect = newRect.ContractedBy(new Padding(padding.Value));

        return newRect;
    }

    public static Rect CenteredOnX(this Rect rect, float width, float height) {
        return new Rect(
            rect.x + (rect.width - width) / 2f,
            rect.y,
            width,
            height
        );
    }

    public static Rect CenteredOnY(this Rect rect, float width, float height) {
        return new Rect(
            rect.x,
            rect.y + (rect.height - height) / 2f,
            width,
            height
        );
    }

    public static Rect CenteredOn(this Rect rect, float width, float height) {
        return new Rect(
            rect.x + (rect.width - width) / 2f,
            rect.y + (rect.height - height) / 2f,
            width,
            height
        );
    }
}