using CosmereFramework.UI;
using UnityEngine;

namespace CosmereFramework.Extension;

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
}