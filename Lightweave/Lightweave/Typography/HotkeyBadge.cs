using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Lightweave.Typography;

public enum HotkeyBadgeSize {
    Small,
    Medium,
}

[Doc(
    Id = "hotkey-badge",
    Summary = "Compact key-cap label for hotkeys: bordered surface, monospace letter, themed muted text.",
    WhenToUse = "Inline hotkey hints next to actions, dock buttons, and key binding rows.",
    SourcePath = "Lightweave/Lightweave/Typography/HotkeyBadge.cs",
    ShowRtl = false
)]
public static class HotkeyBadge {
    public static LightweaveNode Create(
        [DocParam("Key label, e.g. \"N\", \"SPACE\", \"F12\". Will be rendered uppercase.")]
        string key,
        [DocParam("Badge size. Small (~1.25rem) for inline hints, Medium (~1.6rem) for prominent rows.")]
        HotkeyBadgeSize size = HotkeyBadgeSize.Small,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        string label = key?.ToUpperInvariant() ?? string.Empty;
        Rem heightRem = size == HotkeyBadgeSize.Medium ? new Rem(1.6f) : new Rem(1.25f);
        Rem fontRem = size == HotkeyBadgeSize.Medium ? new Rem(0.8125f) : new Rem(0.6875f);
        float minWidthPx = heightRem.ToPixels();
        float horizontalPadPx = new Rem(0.35f).ToPixels();

        LightweaveNode node = NodeBuilder.New($"HotkeyBadge:{label}", line, file);
        node.PreferredHeight = heightRem.ToPixels();

        node.Measure = _ => heightRem.ToPixels();

        node.Paint = (rect, _) => {
            if (Event.current.type != EventType.Repaint) {
                return;
            }

            Theme.Theme theme = RenderContext.Current.Theme;
            int pixelSize = Mathf.RoundToInt(fontRem.ToFontPx());
            Font font = theme.GetFont(FontRole.Mono);
            GUIStyle style = GuiStyleCache.GetOrCreate(font, pixelSize, FontStyle.Normal);
            style.alignment = TextAnchor.MiddleCenter;
            float labelWidth = string.IsNullOrEmpty(label) ? 0f : style.CalcSize(new GUIContent(label)).x;
            float capWidth = Mathf.Max(minWidthPx, labelWidth + horizontalPadPx * 2f);
            float capHeight = heightRem.ToPixels();

            float capX = rect.x + (rect.width - capWidth) * 0.5f;
            float capY = rect.y + (rect.height - capHeight) * 0.5f;
            Rect capRect = RectSnap.Snap(new Rect(capX, capY, capWidth, capHeight));

            PaintBox.Draw(
                capRect,
                BackgroundSpec.Of(ThemeSlot.SurfaceRaised),
                BorderSpec.All(new Rem(0.0625f), ThemeSlot.BorderSubtle),
                RadiusSpec.All(RadiusScale.Sm)
            );

            if (string.IsNullOrEmpty(label)) {
                return;
            }

            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextSecondary);
            GUI.Label(capRect, label, style);
            GUI.color = saved;
        };

        return node;
    }

    [DocVariant("CL_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(() => HotkeyBadge.Create("N"));
    }

    [DocVariant("CL_Playground_Label_Long")]
    public static DocSample DocsLong() {
        return new DocSample(() => HotkeyBadge.Create("SPACE"));
    }

    [DocVariant("CL_Playground_Label_Medium")]
    public static DocSample DocsMedium() {
        return new DocSample(() => HotkeyBadge.Create("F12", HotkeyBadgeSize.Medium));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => HotkeyBadge.Create("L"));
    }
}
