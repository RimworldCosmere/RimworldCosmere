using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Lightweave.Feedback;

[Doc(
    Id = "pill",
    Summary = "Tag-style chip with optional leading icon and label. Supports default / selected / muted states and full-pill click.",
    WhenToUse = "Filter chips, expansion / faction / trait tags, mode toggles. Use Badge for compact non-interactive status.",
    SourcePath = "Lightweave/Lightweave/Feedback/Pill.cs"
)]
public static class Pill {
    public static LightweaveNode Create(
        [DocParam("Display text. Rendered uppercase.")]
        string text,
        [DocParam("Optional leading glyph node (icon, dot, etc).")]
        LightweaveNode? leading = null,
        [DocParam("Visual variant.")]
        PillVariant variant = PillVariant.Default,
        [DocParam("Click handler for the entire pill.")]
        Action? onClick = null,
        [DocParam("Optional tooltip text resolver.")]
        Func<string>? tooltip = null,
        [DocParam("Disable interaction.")]
        bool disabled = false,
        [DocParam("Pill height in rems.")]
        float heightRem = 1.85f,
        [DocParam("Horizontal padding in rems.")]
        float paddingRem = 0.7f,
        [DocParam("Icon size in rems (when leading is provided).")]
        float iconRem = 1.1f,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New($"Pill:{variant}", line, file);
        node.ApplyStyling("pill", style, classes, id);
        node.PreferredHeight = new Rem(heightRem).ToPixels();
        if (leading != null) {
            node.Children.Add(leading);
        }

        string display = text?.ToUpperInvariant() ?? string.Empty;

        node.MeasureWidth = () => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Font font = theme.GetFont(FontRole.Body);
            int px = Mathf.RoundToInt(new Rem(0.65f).ToFontPx());
            GUIStyle style = GuiStyleCache.GetOrCreate(font, px, FontStyle.Bold);
            float labelW = style.CalcSize(new GUIContent(display)).x;
            float padPx = new Rem(paddingRem).ToPixels();
            float gapPx = new Rem(0.4f).ToPixels();
            float iconPx = leading != null ? new Rem(iconRem).ToPixels() + gapPx : 0f;
            return padPx + iconPx + labelW + padPx;
        };

        node.Paint = (rect, paintChildren) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            InteractionState st = InteractionState.Resolve(rect, null, disabled);
            bool hot = !disabled && (st.Hovered || st.Pressed) && onClick != null;

            ThemeSlot bgSlot;
            ThemeSlot borderSlot;
            ThemeSlot textSlot;
            float alpha = 1f;
            switch (variant) {
                case PillVariant.Selected:
                    bgSlot = hot ? ThemeSlot.SurfaceAccent : ThemeSlot.SurfaceRaised;
                    borderSlot = ThemeSlot.BorderHover;
                    textSlot = hot ? ThemeSlot.TextOnAccent : ThemeSlot.TextSecondary;
                    break;
                case PillVariant.Muted:
                    bgSlot = hot ? ThemeSlot.SurfaceAccent : ThemeSlot.SurfaceRaised;
                    borderSlot = ThemeSlot.BorderSubtle;
                    textSlot = hot ? ThemeSlot.TextOnAccent : ThemeSlot.TextMuted;
                    alpha = hot ? 1f : 0.5f;
                    break;
                case PillVariant.Default:
                default:
                    bgSlot = hot ? ThemeSlot.SurfaceAccent : ThemeSlot.SurfaceRaised;
                    borderSlot = ThemeSlot.BorderSubtle;
                    textSlot = hot ? ThemeSlot.TextOnAccent : ThemeSlot.TextSecondary;
                    break;
            }

            Color savedColor = GUI.color;
            if (alpha < 1f) {
                Color bg = theme.GetColor(bgSlot);
                bg.a *= alpha;
                PaintBox.Draw(rect, BackgroundSpec.Of(bg), BorderSpec.All(new Rem(0.0625f), borderSlot), RadiusSpec.All(RadiusScale.Sm));
            }
            else {
                PaintBox.Draw(rect, BackgroundSpec.Of(bgSlot), BorderSpec.All(new Rem(0.0625f), borderSlot), RadiusSpec.All(RadiusScale.Sm));
            }

            float padPx = new Rem(paddingRem).ToPixels();
            float gapPx = new Rem(0.4f).ToPixels();
            float iconPx = new Rem(iconRem).ToPixels();
            float labelStartX = rect.x + padPx;

            if (leading != null) {
                Rect iconRect = new Rect(rect.x + padPx, rect.y + (rect.height - iconPx) * 0.5f, iconPx, iconPx);
                leading.MeasuredRect = iconRect;
                labelStartX = iconRect.xMax + gapPx;
                if (alpha < 1f) {
                    GUI.color = new Color(1f, 1f, 1f, alpha);
                }
            }

            Font font = theme.GetFont(FontRole.Body);
            int px = Mathf.RoundToInt(new Rem(0.65f).ToFontPx());
            GUIStyle style = GuiStyleCache.GetOrCreate(font, px, FontStyle.Bold);
            style.alignment = TextAnchor.MiddleLeft;
            style.clipping = TextClipping.Clip;

            Rect labelRect = new Rect(labelStartX, rect.y, rect.xMax - padPx - labelStartX, rect.height);
            GUI.color = theme.GetColor(textSlot);
            if (alpha < 1f) {
                Color c = GUI.color;
                c.a *= alpha;
                GUI.color = c;
            }
            GUI.Label(RectSnap.Snap(labelRect), display, style);
            GUI.color = savedColor;

            paintChildren();

            if (tooltip != null && Mouse.IsOver(rect)) {
                TooltipHandler.TipRegion(rect, new TipSignal(tooltip, rect.GetHashCode()));
            }

            if (onClick != null && !disabled) {
                MouseoverSounds.DoRegion(rect);
                Event e = Event.current;
                if (e.type == EventType.MouseUp && e.button == 0 && rect.Contains(e.mousePosition)) {
                    onClick.Invoke();
                    SoundDefOf.Click.PlayOneShotOnCamera();
                    e.Use();
                }
            }
        };
        return node;
    }

    [DocVariant("CL_Playground_Feedback_Pill_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(() => Pill.Create("active", onClick: () => { }));
    }

    [DocVariant("CL_Playground_Feedback_Pill_Selected", Order = 1)]
    public static DocSample DocsSelected() {
        return new DocSample(() => Pill.Create("selected", variant: PillVariant.Selected, onClick: () => { }));
    }

    [DocVariant("CL_Playground_Feedback_Pill_Muted", Order = 2)]
    public static DocSample DocsMuted() {
        return new DocSample(() => Pill.Create("muted", variant: PillVariant.Muted, onClick: () => { }));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => Pill.Create("expansion", onClick: () => { }));
    }
}
