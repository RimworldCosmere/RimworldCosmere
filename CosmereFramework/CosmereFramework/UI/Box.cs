using CosmereFramework.Extension;
using UnityEngine;
using Verse;

namespace CosmereFramework.UI;

public static class Box {
    public static Rect Create(
        float x,
        float y,
        float width,
        float height,
        Padding padding,
        Texture2D? border = null,
        int borderThickness = 1
    ) {
        Rect rect = new Rect(x, y, width, height);

        if (border != null) {
            Widgets.DrawBox(rect, borderThickness, border);
        }

        return rect.ContractedBy(padding);
    }
}