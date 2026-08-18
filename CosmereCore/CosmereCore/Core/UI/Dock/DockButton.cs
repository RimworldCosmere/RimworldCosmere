using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.UI.Dock;

/// <summary>
///     Which job a button is doing. The distinction is the point: identically weighted buttons
///     make the player read both every time, where a filled primary beside a ghost says which is the ordinary move.
/// </summary>
public enum DockButtonKind {
    Primary,
    Ghost,
    Active,
}

/// <summary>
///     A dock action. Vanilla's tan gradient fights both shardworld palettes, so these take the
///     section's own accent and the dock's rounded panel instead.
/// </summary>
public static class DockButton {
    /// <summary>
    ///     Slow on purpose. The metal tiles pulse at 6f for flaring, which is an alert; a moment
    ///     the player has earned should breathe rather than blink.
    /// </summary>
    private const float PulseRate = 2f;

    private static readonly Color Disabled = new Color(0.145f, 0.157f, 0.169f);
    private static readonly Color DisabledText = new Color(0.310f, 0.325f, 0.337f);
    private static readonly Color GhostFill = new Color(0.086f, 0.098f, 0.110f, 0.55f);

    public static bool Draw(
        Rect rect,
        string label,
        Color accent,
        bool pulse = false,
        DockButtonKind kind = DockButtonKind.Ghost,
        bool enabled = true
    ) {
        bool over = enabled && Mouse.IsOver(rect);

        Color fill = !enabled
            ? Disabled
            : kind switch {
                DockButtonKind.Primary => over ? Lighten(accent, 0.12f) : accent,
                DockButtonKind.Active => new Color(accent.r, accent.g, accent.b, 0.22f),
                _ => GhostFill,
            };

        Color border = enabled ? accent : Disabled;

        // Filled buttons use dark lettering - white on a lit accent goes illegible as it brightens.
        Color text = !enabled
            ? DisabledText
            : kind == DockButtonKind.Primary
                ? new Color(0.090f, 0.075f, 0.047f)
                : new Color(0.851f, 0.878f, 0.902f);

        Panel.Draw(rect, fill, border);

        if (pulse) {
            float wash = 0.20f + Mathf.Sin(Time.realtimeSinceStartup * PulseRate) * 0.10f;
            Color previous = GUI.color;
            GUI.color = new Color(accent.r, accent.g, accent.b, wash);
            Widgets.DrawAtlas(rect, DockTex.RoundFill);
            GUI.color = previous;
        }

        UIText.EllipsisLabel(rect, label, GameFont.Small, TextAnchor.MiddleCenter, text);

        if (enabled) {
            if (kind != DockButtonKind.Primary) Panel.Hover(rect, accent);
            MouseoverSounds.DoRegion(rect);
        }

        return enabled && Widgets.ButtonInvisible(rect);
    }

    private static Color Lighten(Color color, float amount) {
        return new Color(
            color.r + (1f - color.r) * amount,
            color.g + (1f - color.g) * amount,
            color.b + (1f - color.b) * amount,
            color.a
        );
    }
}
