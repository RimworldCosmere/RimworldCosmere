using System;
using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Lightweave.Input;

public static class ColorPicker {
    private static readonly Color[] DefaultPalette = new[] {
        new Color(0.88f, 0.95f, 0.98f),
        new Color(0.62f, 0.62f, 0.65f),
        new Color(0.82f, 0.82f, 0.78f),
        new Color(0.72f, 0.45f, 0.28f),
        new Color(0.20f, 0.28f, 0.55f),
        new Color(0.25f, 0.42f, 0.30f),
        new Color(0.68f, 0.20f, 0.52f),
        new Color(0.82f, 0.68f, 0.38f),
        new Color(0.62f, 0.18f, 0.22f),
        new Color(0.78f, 0.55f, 0.25f),
    };

    public static LightweaveNode Create(
        Color value,
        Action<Color> onChange,
        IReadOnlyList<Color>? palette = null,
        bool disabled = false,
        [CallerFilePath] string? caller = null,
        [CallerLineNumber] int line = 0
    ) {
        LightweaveNode node = NodeBuilder.New("ColorPicker", line, caller ?? string.Empty);

        IReadOnlyList<Color> effectivePalette = palette ?? DefaultPalette;

        node.Measure = availableWidth => {
            if (effectivePalette.Count == 0) {
                return new Rem(1.5f).ToPixels();
            }

            float swatchSize = new Rem(1.5f).ToPixels();
            float gap = SpacingScale.Xs.ToPixels();
            int columnsPerRow = Mathf.Max(1, Mathf.FloorToInt((availableWidth + gap) / (swatchSize + gap)));
            int rows = Mathf.CeilToInt((float)effectivePalette.Count / columnsPerRow);
            return rows * swatchSize + Mathf.Max(0, rows - 1) * gap;
        };

        node.Paint = (rect, paintChildren) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;

            if (effectivePalette.Count == 0) {
                DrawEmptyLabel(rect, theme);
                paintChildren();
                return;
            }

            float swatchSize = new Rem(1.5f).ToPixels();
            float gap = SpacingScale.Xs.ToPixels();
            int columnsPerRow = Mathf.Max(1, Mathf.FloorToInt((rect.width + gap) / (swatchSize + gap)));

            Event e = Event.current;
            for (int i = 0; i < effectivePalette.Count; i++) {
                int row = i / columnsPerRow;
                int col = i % columnsPerRow;

                float xOffset;
                if (dir == Direction.Rtl) {
                    int rtlCol = columnsPerRow - 1 - col;
                    xOffset = rtlCol * (swatchSize + gap);
                } else {
                    xOffset = col * (swatchSize + gap);
                }

                float y = rect.y + row * (swatchSize + gap);
                Rect swatchRect = new Rect(rect.x + xOffset, y, swatchSize, swatchSize);

                if (swatchRect.yMax > rect.yMax) {
                    break;
                }

                Color swatchColor = effectivePalette[i];
                bool selected = ColorsApproximatelyEqual(swatchColor, value);
                bool isOverSwatch = Mouse.IsOver(swatchRect);
                if (disabled && isOverSwatch) {
                    CursorOverrides.MarkDisabledHover();
                }

                bool hovered = !disabled && isOverSwatch;

                DrawSwatch(swatchRect, swatchColor, theme, selected, hovered, disabled);

                if (!disabled && e.type == EventType.MouseUp && e.button == 0 && swatchRect.Contains(e.mousePosition)) {
                    onChange?.Invoke(swatchColor);
                    e.Use();
                }
            }

            paintChildren();
        };

        return node;
    }

    private static void DrawSwatch(
        Rect rect,
        Color color,
        Theme.Theme theme,
        bool selected,
        bool hovered,
        bool disabled
    ) {
        float alpha = disabled ? 0.5f : 1f;
        Color drawColor = new Color(color.r, color.g, color.b, color.a * alpha);

        ThemeSlot borderSlot;
        Rem borderWidth;
        if (selected) {
            borderSlot = ThemeSlot.BorderFocus;
            borderWidth = new Rem(2f / 16f);
        } else if (hovered) {
            borderSlot = ThemeSlot.BorderHover;
            borderWidth = new Rem(2f / 16f);
        } else {
            borderSlot = ThemeSlot.BorderSubtle;
            borderWidth = new Rem(1f / 16f);
        }

        BackgroundSpec bg = new BackgroundSpec.Solid(drawColor);
        BorderSpec border = BorderSpec.All(borderWidth, borderSlot);
        RadiusSpec radius = RadiusSpec.All(new Rem(0.125f));
        PaintBox.Draw(rect, bg, border, radius);

        if (selected) {
            float insetPx = 2f;
            Rect inset = new Rect(
                rect.x + insetPx,
                rect.y + insetPx,
                rect.width - insetPx * 2f,
                rect.height - insetPx * 2f
            );
            BorderSpec insetBorder = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.TextOnAccent);
            RadiusSpec insetRadius = RadiusSpec.All(new Rem(0.0625f));
            PaintBox.Draw(inset, null, insetBorder, insetRadius);
        }
    }

    private static void DrawEmptyLabel(Rect rect, Theme.Theme theme) {
        Font font = theme.GetFont(FontRole.Body);
        int pixelSize = Mathf.RoundToInt(new Rem(1f).ToFontPx());
        GUIStyle style = GuiStyleCache.Get(font, pixelSize, FontStyle.Italic);
        style.alignment = TextAnchor.MiddleCenter;

        Color saved = GUI.color;
        GUI.color = theme.GetColor(ThemeSlot.TextMuted);
        GUI.Label(RectSnap.Snap(rect), (string)"CC_Lightweave_ColorPicker_NoColors".Translate(), style);
        GUI.color = saved;
    }

    private static bool ColorsApproximatelyEqual(Color a, Color b) {
        return Mathf.Abs(a.r - b.r) < 0.01f &&
               Mathf.Abs(a.g - b.g) < 0.01f &&
               Mathf.Abs(a.b - b.b) < 0.01f &&
               Mathf.Abs(a.a - b.a) < 0.01f;
    }
}