using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Dock;

public static class DockRows {
    public const float SlimRowHeight = 22f;
    public const float GroupLabelHeight = 16f;
    public const float RichCellHeight = 64f;
    private const float SlimIconSize = 14f;
    private const float RichIconSize = 20f;
    private const float AxisColumnWidth = 22f;

    public static void DrawGroupLabel(Rect rect, string label, bool hot) {
        Color color = hot ? DockPalette.HotLabel : DockPalette.GroupLabel;
        Rect textRect = rect.ContractedBy(10f, 0f);
        Vector2 size;
        using (new TextBlock(GameFont.Tiny)) size = Text.CalcSize(label);
        UIText.EllipsisLabel(textRect, label, GameFont.Tiny, TextAnchor.MiddleLeft, color);

        float ruleStart = textRect.x + Mathf.Min(size.x, textRect.width) + 6f;
        if (ruleStart < rect.xMax - 12f) {
            Rect rule = new Rect(ruleStart, rect.center.y, rect.xMax - 10f - ruleStart, 1f);
            Widgets.DrawBoxSolid(rule, new Color(color.r, color.g, color.b, 0.45f));
        }
    }

    public static bool DrawSlimRow(
        Rect rect,
        Texture2D? icon,
        string label,
        string axisGlyph,
        float fraction,
        ISystemSkin skin,
        bool ghosted
    ) {
        Color prev = GUI.color;
        if (ghosted) GUI.color = new Color(1f, 1f, 1f, DockPalette.GhostOpacity);

        if (!ghosted && Mouse.IsOver(rect)) Widgets.DrawBoxSolid(rect, DockPalette.PanelRaised);

        Rect iconRect = new Rect(rect.x + 10f, rect.center.y - SlimIconSize / 2f, SlimIconSize, SlimIconSize);
        if (icon != null) GUI.DrawTexture(iconRect, icon);

        Rect nameRect = new Rect(iconRect.xMax + 7f, rect.y, 66f, rect.height);
        UIText.EllipsisLabel(nameRect, label, GameFont.Tiny, TextAnchor.MiddleLeft, DockPalette.MutedText);

        Rect axisRect = new Rect(nameRect.xMax, rect.y, AxisColumnWidth, rect.height);
        UIText.EllipsisLabel(axisRect, axisGlyph, GameFont.Tiny, TextAnchor.MiddleCenter, DockPalette.GroupLabel);

        Rect barRect = new Rect(axisRect.xMax + 4f, rect.center.y - 2f, rect.xMax - axisRect.xMax - 14f, 4f);
        HorizontalBar.Draw(barRect, fraction, null, skin.BarBackgroundColor, skin.BarFillColor, Color.clear);

        Rect divider = new Rect(rect.x + 8f, rect.yMax - 1f, rect.width - 16f, 1f);
        Widgets.DrawBoxSolid(divider, new Color(DockPalette.Border.r, DockPalette.Border.g, DockPalette.Border.b, 0.14f));

        GUI.color = prev;

        if (ghosted) return false;
        Widgets.DrawHighlightIfMouseover(rect);
        return Widgets.ButtonInvisible(rect);
    }

    public static Rect BeginRichCell(
        Rect rect,
        Texture2D? icon,
        string name,
        string stateLabel,
        Color stateColor,
        float fraction,
        float? targetFraction,
        ISystemSkin skin,
        bool flaring
    ) {
        Widgets.DrawBoxSolid(rect, DockPalette.PanelRaised);
        Color edge = flaring ? DockPalette.FlareBorder : skin.AccentColor;
        Widgets.DrawBoxSolid(new Rect(rect.x, rect.y, 2f, rect.height), edge);
        Widgets.DrawBoxSolidWithOutline(rect, Color.clear, DockPalette.BorderSubtle);

        Rect inner = rect.ContractedBy(8f, 6f);
        Rect iconRect = new Rect(inner.x, inner.y, RichIconSize, RichIconSize);
        if (icon != null) GUI.DrawTexture(iconRect, icon);

        Rect stateRect = new Rect(inner.xMax - 70f, inner.y, 70f, RichIconSize);
        UIText.EllipsisLabel(stateRect, stateLabel, GameFont.Tiny, TextAnchor.MiddleRight, stateColor);

        Rect nameRect = new Rect(iconRect.xMax + 7f, inner.y, stateRect.x - iconRect.xMax - 10f, RichIconSize);
        UIText.EllipsisLabel(nameRect, name, GameFont.Small, TextAnchor.MiddleLeft, Color.white);

        Rect barRect = new Rect(inner.x, iconRect.yMax + 4f, inner.width, 7f);
        Color fill = flaring ? DockPalette.Flare : skin.BarFillColor;
        HorizontalBar.Draw(barRect, fraction, targetFraction, skin.BarBackgroundColor, fill, new Color(1f, 1f, 1f, 0.8f));

        return new Rect(inner.x, barRect.yMax + 5f, inner.width, 20f);
    }
}
