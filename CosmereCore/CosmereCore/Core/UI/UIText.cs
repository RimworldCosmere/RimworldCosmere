using UnityEngine;
using Verse;

namespace Cosmere.Core.UI;

public static class UIText {
    public static bool Fits(string text, float width, GameFont font) {
        using (new TextBlock(font)) {
            return Text.CalcSize(text).x <= width;
        }
    }

    /// <summary>
    ///     Draws <paramref name="text" /> wrapped onto as many lines as it needs, centred vertically on
    ///     <paramref name="centre" />, and returns the rect it used.
    /// </summary>
    /// <remarks>
    ///     For places that have limited width but plenty of height - a radial wedge, say - where cutting
    ///     a label down to "Summon Sh..." loses the very thing the label is for.
    /// </remarks>
    public static Rect WrappedLabel(
        Vector2 centre,
        float width,
        string text,
        GameFont font,
        TextAnchor anchor,
        Color color) {
        using (new TextBlock(font, anchor, color)) {
            Text.WordWrap = true;
            float height = Text.CalcHeight(text, width);
            Rect rect = new Rect(centre.x - width / 2f, centre.y - height / 2f, width, height);
            Widgets.Label(rect, text);
            return rect;
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
