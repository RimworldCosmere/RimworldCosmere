using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Cosmere.Lightweave.Typography;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.MainMenu;

public static class DockTile {
    private static readonly Rem TileHeight = new Rem(4.75f);

    public static LightweaveNode Create(
        string label,
        string hotkey,
        Action onClick,
        bool disabled = false,
        bool chevron = false,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode body = NodeBuilder.New($"DockTile:body:{label}", line, file);
        body.Paint = (rect, _) => {
            DrawLabel(rect, label, disabled);
            if (chevron) {
                DrawChevron(rect, disabled);
            }
            else {
                DrawHotkeyHint(rect, hotkey, disabled);
            }
        };

        return Button.Create(
            label: string.Empty,
            onClick: onClick,
            variant: ButtonVariant.Dock,
            disabled: disabled,
            style: new Style { Width = Length.Stretch, Height = TileHeight },
            body: body,
            line: line,
            file: file
        );
    }

    private static void DrawLabel(Rect tile, string label, bool disabled) {
        if (Event.current.type != EventType.Repaint) {
            return;
        }

        Theme.Theme theme = RenderContext.Current.Theme;
        Font font = theme.GetFont(FontRole.Body);
        int pixelSize = Mathf.RoundToInt(new Rem(0.875f).ToFontPx());
        GUIStyle style = GuiStyleCache.GetOrCreate(font, pixelSize, FontStyle.Normal);
        style.alignment = TextAnchor.MiddleCenter;

        string upper = (label ?? string.Empty).ToUpperInvariant();
        float labelH = new Rem(1.3f).ToPixels();
        Rect labelRect = new Rect(
            tile.x,
            tile.y + (tile.height - labelH) * 0.5f - new Rem(0.35f).ToPixels(),
            tile.width,
            labelH
        );

        Color saved = GUI.color;
        bool hovered = !disabled && tile.Contains(Event.current.mousePosition);
        ThemeSlot fgSlot = disabled
            ? ThemeSlot.TextMuted
            : hovered
                ? ThemeSlot.TextOnAccent
                : ThemeSlot.TextPrimary;
        GUI.color = theme.GetColor(fgSlot);

        float trackedAdvance = pixelSize * 0.04f;
        float totalW = 0f;
        for (int i = 0; i < upper.Length; i++) {
            GUIContent ch = new GUIContent(upper[i].ToString());
            totalW += style.CalcSize(ch).x;
            if (i < upper.Length - 1) totalW += trackedAdvance;
        }

        float startX = labelRect.x + (labelRect.width - totalW) * 0.5f;
        float cursor = startX;
        for (int i = 0; i < upper.Length; i++) {
            string ch = upper[i].ToString();
            GUIContent gcc = new GUIContent(ch);
            float w = style.CalcSize(gcc).x;
            GUI.Label(RectSnap.Snap(new Rect(cursor, labelRect.y, w, labelRect.height)), ch, style);
            cursor += w + trackedAdvance;
        }

        GUI.color = saved;
    }

    private static void DrawHotkeyHint(Rect tile, string key, bool disabled) {
        if (string.IsNullOrEmpty(key)) {
            return;
        }

        if (Event.current.type != EventType.Repaint) {
            return;
        }

        Theme.Theme theme = RenderContext.Current.Theme;
        Font font = theme.GetFont(FontRole.Mono);
        int pixelSize = Mathf.RoundToInt(new Rem(0.7f).ToFontPx());
        GUIStyle style = GuiStyleCache.GetOrCreate(font, pixelSize, FontStyle.Normal);
        style.alignment = TextAnchor.MiddleCenter;

        string label = "[" + key.ToUpperInvariant() + "]";
        GUIContent gc = new GUIContent(label);
        float w = style.CalcSize(gc).x;
        float h = new Rem(1.0f).ToPixels();
        Rect hint = RectSnap.Snap(new Rect(
            tile.x + (tile.width - w) * 0.5f,
            tile.yMax - h - new Rem(0.4f).ToPixels(),
            w,
            h
        ));

        Color saved = GUI.color;
        bool hovered = !disabled && tile.Contains(Event.current.mousePosition);
        ThemeSlot fgSlot = disabled
            ? ThemeSlot.TextMuted
            : hovered
                ? ThemeSlot.TextOnAccent
                : ThemeSlot.TextSecondary;
        GUI.color = theme.GetColor(fgSlot);
        GUI.Label(hint, label, style);
        GUI.color = saved;
    }

    private static void DrawChevron(Rect tile, bool disabled) {
        if (Event.current.type != EventType.Repaint) {
            return;
        }

        Theme.Theme theme = RenderContext.Current.Theme;
        bool hovered = !disabled && tile.Contains(Event.current.mousePosition);
        ThemeSlot fgSlot = disabled
            ? ThemeSlot.TextMuted
            : hovered
                ? ThemeSlot.TextOnAccent
                : ThemeSlot.TextSecondary;
        Color tint = theme.GetColor(fgSlot);

        float armLen = new Rem(0.42f).ToPixels();
        float gap = new Rem(0.32f).ToPixels();
        float thickness = Mathf.Max(1f, new Rem(0.0875f).ToPixels());
        float cx = tile.x + tile.width * 0.5f;
        float cy = tile.yMax - new Rem(0.85f).ToPixels();
        DrawDiagonal(cx - gap, cy - thickness * 0.5f, cx, cy + armLen - thickness * 0.5f, thickness, tint);
        DrawDiagonal(cx + gap, cy - thickness * 0.5f, cx, cy + armLen - thickness * 0.5f, thickness, tint);
    }

    private static void DrawDiagonal(float x0, float y0, float x1, float y1, float thickness, Color color) {
        Color saved = GUI.color;
        GUI.color = color;
        float dx = x1 - x0;
        float dy = y1 - y0;
        float len = Mathf.Sqrt(dx * dx + dy * dy);
        if (len < 0.001f) {
            GUI.color = saved;
            return;
        }
        Matrix4x4 prev = GUI.matrix;
        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
        Vector2 pivot = new Vector2(x0, y0);
        GUIUtility.RotateAroundPivot(angle, pivot);
        GUI.DrawTexture(RectSnap.Snap(new Rect(x0, y0, len, thickness)), BaseContent.WhiteTex);
        GUI.matrix = prev;
        GUI.color = saved;
    }
}
