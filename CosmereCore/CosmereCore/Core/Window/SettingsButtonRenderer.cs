using Cosmere.Core.UI;
using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.Window;

/// <summary>
///     Vanilla's button atlas is parchment-on-wood with no system identity. These keep the
///     vanilla silhouette but fill from the active shard accent, so the footer belongs to its window.
/// </summary>
public static class SettingsButtonRenderer {
    private const float BorderThickness = 1f;

    private static readonly Color DestructiveFill = new Color(0.30f, 0.12f, 0.11f);
    private static readonly Color DestructiveBorder = new Color(0.70f, 0.27f, 0.23f);
    private static readonly Color DestructiveText = new Color(0.93f, 0.71f, 0.67f);
    private static readonly Color NeutralFill = new Color(1f, 1f, 1f, 0.045f);
    private static readonly Color NeutralBorder = new Color(1f, 1f, 1f, 0.18f);
    private static readonly Color NeutralText = new Color(0.76f, 0.79f, 0.83f);

    public static bool Draw(Rect rect, string label, ISystemSkin skin, SettingsButtonStyle style) {
        bool hovered = Mouse.IsOver(rect);
        ResolveColors(style, skin, out Color fill, out Color border, out Color text);

        if (hovered) {
            fill = Brighten(fill, 0.06f);
            border = Brighten(border, 0.15f);
        }

        Widgets.DrawBoxSolid(rect, fill);

        // a single lit top edge sells a flat rect as pressable, without a gradient.
        Widgets.DrawBoxSolid(
            new Rect(rect.x + BorderThickness, rect.y + BorderThickness, rect.width - BorderThickness * 2f, BorderThickness),
            new Color(1f, 1f, 1f, hovered ? 0.16f : 0.09f)
        );

        Color previousColor = GUI.color;
        GUI.color = border;
        try {
            Widgets.DrawBox(rect, (int)BorderThickness);
        } finally {
            GUI.color = previousColor;
        }

        UIText.EllipsisLabel(rect.ContractedBy(8f, 0f), label, GameFont.Small, TextAnchor.MiddleCenter, text);
        TooltipHandler.TipRegion(rect, label);
        MouseoverSounds.DoRegion(rect);

        return Widgets.ButtonInvisible(rect);
    }

    private static void ResolveColors(
        SettingsButtonStyle style,
        ISystemSkin skin,
        out Color fill,
        out Color border,
        out Color text
    ) {
        switch (style) {
            case SettingsButtonStyle.Destructive:
                fill = DestructiveFill;
                border = DestructiveBorder;
                text = DestructiveText;

                return;
            case SettingsButtonStyle.Primary:
                fill = new Color(skin.AccentColor.r, skin.AccentColor.g, skin.AccentColor.b, 0.22f);
                border = skin.AccentColor;
                text = skin.HeaderTextColor;

                return;
            default:
                fill = NeutralFill;
                border = NeutralBorder;
                text = NeutralText;

                return;
        }
    }

    private static Color Brighten(Color color, float amount) {
        return new Color(
            Mathf.Min(1f, color.r + amount),
            Mathf.Min(1f, color.g + amount),
            Mathf.Min(1f, color.b + amount),
            Mathf.Min(1f, color.a + amount)
        );
    }
}
