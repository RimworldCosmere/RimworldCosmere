using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Codex;

public static class CodexChrome {
    public const float HeaderHeight = 36f;
    public const float RailWidth = 40f;
    public const float SubtabBarHeight = 28f;
    public const float Gutter = 8f;

    public static void DrawHeader(Rect rect, string label, Color accent) {
        Widgets.DrawBoxSolid(rect, new Color(0.08f, 0.08f, 0.1f, 0.85f));
        Rect accentLine = new Rect(rect.x, rect.yMax - 2f, rect.width, 2f);
        Widgets.DrawBoxSolid(accentLine, accent);
        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white)) {
            Rect labelRect = new Rect(rect.x + Gutter, rect.y, rect.width - Gutter * 2f, rect.height);
            Widgets.Label(labelRect, label);
        }
    }

    public static void DrawDivider(Rect rect, Color accent) {
        Widgets.DrawBoxSolid(rect, new Color(accent.r, accent.g, accent.b, 0.35f));
    }

    public static Rect BodyRect(Rect tabRect, bool hasSwitcher) {
        float top = HeaderHeight + SubtabBarHeight;
        float left = hasSwitcher ? RailWidth : 0f;

        // No horizontal gutter: the body runs to the frame, and content sets its
        // own inset where it wants one.
        return new Rect(
            tabRect.x + left,
            tabRect.y + top + Gutter,
            tabRect.width - left,
            tabRect.height - top - Gutter * 2f
        );
    }
}
