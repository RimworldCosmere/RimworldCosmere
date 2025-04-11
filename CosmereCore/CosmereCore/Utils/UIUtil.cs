using System;
using UnityEngine;
using Verse;

namespace CosmereCore.Utils
{
    public class UIUtil
    {
        public static void WithAnchor(TextAnchor anchor, Action draw)
        {
            var prev = Text.Anchor;
            Text.Anchor = anchor;
            draw();
            Text.Anchor = prev;
        }

        public static void WithFont(GameFont font, Action draw)
        {
            var prev = Text.Font;
            Text.Font = font;
            draw();
            Text.Font = prev;
        }

        public static void WithColor(Color color, Action draw)
        {
            var prev = GUI.color;
            GUI.color = color;
            draw();
            GUI.color = prev;
        }

        public static Color GetBestTextColor(Color background)
        {
            var luminance = 0.299f * background.r + 0.587f * background.g + 0.114f * background.b;
            return luminance > 0.5f ? Color.black : Color.white;
        }

        public static void DrawContrastLabel(Rect rect, string text, Color overlayColor)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Small;

            var originalColor = GUI.color;

            // Draw base label as black (outline)
            GUI.color = Color.black;
            Widgets.Label(new Rect(rect.x + 1, rect.y + 1, rect.width + 1, rect.height + 1), text);

            // Draw foreground label as metal color
            GUI.color = Color.white;
            Widgets.Label(rect, text);

            GUI.color = originalColor;
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}