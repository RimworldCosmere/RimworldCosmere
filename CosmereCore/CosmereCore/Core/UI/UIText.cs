using UnityEngine;
using Verse;

namespace Cosmere.Core.UI;

public static class UIText {
    public static bool Fits(string text, float width, GameFont font) {
        using (new TextBlock(font)) {
            return Text.CalcSize(text).x <= width;
        }
    }

    public static void EllipsisLabel(Rect rect, string text, GameFont font, TextAnchor anchor, Color color) {
        using (new TextBlock(font, anchor, color)) {
            if (Text.CalcSize(text).x <= rect.width) {
                Widgets.Label(rect, text);
                return;
            }

            string truncated = text.Truncate(rect.width);
            Widgets.Label(rect, truncated);
            TooltipHandler.TipRegion(rect, text);
        }
    }
}
