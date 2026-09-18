using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.UI.Dock;

/// <summary>
///     A dock action. One surface for every button: state is carried by the label's weight and
///     colour, never by a coloured fill or border.
/// </summary>
public static class DockButton {
    /// <summary>
    ///     Slow on purpose. The metal tiles pulse at 6f for flaring, which is an alert; a moment
    ///     the player has earned should breathe rather than blink.
    /// </summary>
    private const float PulseRate = 2f;

    private const float IconSize = 20f;
    private const float Inset = 6f;

    private static readonly Color Disabled = new Color(0.145f, 0.157f, 0.169f);
    private static readonly Color DisabledText = new Color(0.310f, 0.325f, 0.337f);
    private static readonly Color GhostFill = new Color(0.086f, 0.098f, 0.110f, 0.55f);
    private static readonly Color LabelText = new Color(0.851f, 0.878f, 0.902f);

    public static bool Draw(
        Rect rect,
        string label,
        Color accent,
        bool pulse = false,
        bool active = false,
        bool enabled = true,
        Texture2D? icon = null,
        string? aside = null
    ) {
        Color text = !enabled
            ? DisabledText
            : active
                ? accent
                : LabelText;

        // the oath button breathes on the lettering now that no button carries a coloured wash
        if (enabled && pulse) {
            float t = 0.5f + Mathf.Sin(Time.realtimeSinceStartup * PulseRate) * 0.5f;
            text = Color.Lerp(text, accent, t);
        }

        Panel.Draw(rect, enabled ? GhostFill : Disabled, enabled ? DockPalette.BorderSubtle : Disabled);

        Rect labelRect = rect;

        if (icon != null) {
            Rect iconRect = new Rect(
                rect.x + Inset,
                rect.y + (rect.height - IconSize) / 2f,
                IconSize,
                IconSize
            );
            GUI.DrawTexture(iconRect, icon);
            labelRect = new Rect(iconRect.xMax + Inset, rect.y, rect.width - (iconRect.xMax + Inset - rect.x), rect.height);
        }

        if (!aside.NullOrEmpty()) {
            float asideWidth = rect.width * 0.35f;
            UIText.EllipsisLabel(
                new Rect(rect.xMax - Inset - asideWidth, rect.y, asideWidth, rect.height),
                aside!,
                GameFont.Tiny,
                TextAnchor.MiddleRight,
                DockPalette.MutedText
            );
            labelRect = new Rect(labelRect.x, labelRect.y, labelRect.width - asideWidth - Inset, labelRect.height);
        }

        UIText.EllipsisLabel(
            labelRect,
            label,
            GameFont.Small,
            icon == null && aside.NullOrEmpty() ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft,
            text,
            active
        );

        if (enabled) {
            Panel.Hover(rect, DockPalette.Border);
            MouseoverSounds.DoRegion(rect);
        }

        return enabled && Widgets.ButtonInvisible(rect);
    }
}
