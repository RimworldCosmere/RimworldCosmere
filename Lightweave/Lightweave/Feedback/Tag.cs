using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Feedback;

[Doc(
    Id = "tag",
    Summary = "Rectangular bordered chip with monospace uppercase tracked label and optional leading dot. Static, non-interactive.",
    WhenToUse = "Save list AUTO chip, status indicator (MANUAL SAVE / AUTOSAVE / INCOMPATIBLE), active condition rows. Use Pill for interactive filter chips; use Badge for pill-shaped status pills.",
    SourcePath = "Lightweave/Lightweave/Feedback/Tag.cs"
)]
public static class Tag {
    public static LightweaveNode Create(
        [DocParam("Display text. Rendered uppercase, tracked.")]
        string text,
        [DocParam("Foreground (text + dot) color slot. Border defaults to this slot unless overridden.")]
        ThemeSlot textColor = ThemeSlot.TextMuted,
        [DocParam("Border color slot. When null, falls back to BorderSubtle.")]
        ThemeSlot? borderColor = null,
        [DocParam("Show a leading colored dot before the label.")]
        bool dot = false,
        [DocParam("Override dot color slot. When null, dot inherits textColor.")]
        ThemeSlot? dotColor = null,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New($"Tag:{text}", line, file);
        node.ApplyStyling("tag", style, classes, id);
        node.PreferredHeight = new Rem(1.25f).ToPixels();

        string display = text?.ToUpperInvariant() ?? string.Empty;
        ThemeSlot resolvedBorder = borderColor ?? ThemeSlot.BorderSubtle;
        ThemeSlot resolvedDot = dotColor ?? textColor;

        float TrackedWidth(GUIStyle gs, float tracking) {
            float total = 0f;
            for (int i = 0; i < display.Length; i++) {
                GUIContent ch = new GUIContent(display[i].ToString());
                total += gs.CalcSize(ch).x;
                if (i < display.Length - 1) total += tracking;
            }
            return total;
        }

        node.MeasureWidth = () => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Font font = theme.GetFont(FontRole.Mono);
            int px = Mathf.Max(10, Mathf.RoundToInt(new Rem(0.65f).ToFontPx()));
            GUIStyle gs = GuiStyleCache.GetOrCreate(font, px, FontStyle.Normal);
            float padX = new Rem(0.5f).ToPixels();
            float dotSize = new Rem(0.375f).ToPixels();
            float dotGap = new Rem(0.375f).ToPixels();
            float tracking = px * 0.1f;
            float labelW = TrackedWidth(gs, tracking);
            float dotW = dot ? dotSize + dotGap : 0f;
            return padX + dotW + labelW + padX;
        };

        node.Paint = (outerRect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Font font = theme.GetFont(FontRole.Mono);
            int px = Mathf.Max(10, Mathf.RoundToInt(new Rem(0.65f).ToFontPx()));
            GUIStyle gs = GuiStyleCache.GetOrCreate(font, px, FontStyle.Normal);
            gs.alignment = TextAnchor.MiddleLeft;
            gs.clipping = TextClipping.Clip;

            float padX = new Rem(0.5f).ToPixels();
            float dotSize = new Rem(0.375f).ToPixels();
            float dotGap = new Rem(0.375f).ToPixels();
            float tracking = px * 0.1f;
            float labelW = TrackedWidth(gs, tracking);
            float dotW = dot ? dotSize + dotGap : 0f;
            float natural = padX + dotW + labelW + padX;

            float width = Mathf.Min(outerRect.width, natural);
            Rect rect = new Rect(outerRect.x, outerRect.y, width, outerRect.height);

            PaintBox.Draw(
                rect,
                null,
                BorderSpec.All(new Rem(0.0625f), resolvedBorder),
                RadiusSpec.All(RadiusScale.None)
            );

            float cursorX = rect.x + padX;
            Color saved = GUI.color;

            if (dot) {
                Rect dotRect = new Rect(
                    cursorX,
                    rect.y + (rect.height - dotSize) * 0.5f,
                    dotSize,
                    dotSize
                );
                PaintBox.Draw(
                    dotRect,
                    BackgroundSpec.Of(resolvedDot),
                    null,
                    RadiusSpec.All(RadiusScale.Full)
                );
                cursorX += dotSize + dotGap;
            }

            GUI.color = theme.GetColor(textColor);
            float labelRight = rect.xMax - padX;
            float cursor = cursorX;
            for (int i = 0; i < display.Length; i++) {
                string ch = display[i].ToString();
                GUIContent gc = new GUIContent(ch);
                float w = gs.CalcSize(gc).x;
                if (cursor + w > labelRight) break;
                GUI.Label(RectSnap.Snap(new Rect(cursor, rect.y, w, rect.height)), ch, gs);
                cursor += w + tracking;
            }
            GUI.color = saved;
        };

        return node;
    }
}
