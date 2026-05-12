using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Hooks;
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
    private static readonly Rem TileHeight = new Rem(5.6875f);

    public static LightweaveNode Create(
        string label,
        string hotkey,
        Action onClick,
        bool disabled = false,
        bool chevron = false,
        bool expanded = false,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        if (!disabled && !string.IsNullOrEmpty(hotkey) && TryParseHotkey(hotkey, out KeyCode code)) {
            UseHotkey.Use(code, onClick);
        }

        Hooks.Hooks.RefHandle<float> flipRatio = Hooks.Hooks.UseRef<float>(expanded ? 1f : 0f, line, file + "#dock-flip");

        LightweaveNode body = NodeBuilder.New($"DockTile:body:{label}", line, file);
        body.Paint = (rect, _) => {
            if (chevron) {
                float target = expanded ? 1f : 0f;
                float dt = Time.unscaledDeltaTime;
                flipRatio.Current = Mathf.MoveTowards(flipRatio.Current, target, dt / 0.16f);
            }
            DrawLabel(rect, label, disabled);
            if (chevron) {
                DrawChevron(rect, disabled, flipRatio.Current);
            }
            else {
                DrawHotkeyHint(rect, hotkey, disabled);
            }
        };

        Style baseStyle = new Style { Width = Length.Stretch, Height = TileHeight };
        Style merged = style.HasValue ? Style.Merge(baseStyle, style.Value) : baseStyle;

        return Button.Create(
            label: string.Empty,
            onClick: onClick,
            variant: ButtonVariant.Frosted,
            disabled: disabled,
            style: merged,
            classes: StyleExtensions.PrependClass("dock-tile", classes),
            id: id,
            body: body,
            line: line,
            file: file
        );
    }

    private static bool TryParseHotkey(string hotkey, out KeyCode code) {
        if (hotkey.Length == 1) {
            char c = char.ToUpperInvariant(hotkey[0]);
            if (c >= 'A' && c <= 'Z') {
                code = (KeyCode)((int)KeyCode.A + (c - 'A'));
                return true;
            }
            if (c >= '0' && c <= '9') {
                code = (KeyCode)((int)KeyCode.Alpha0 + (c - '0'));
                return true;
            }
        }
        code = KeyCode.None;
        return false;
    }

    private static void DrawLabel(Rect tile, string label, bool disabled) {
        if (Event.current.type != EventType.Repaint) {
            return;
        }

        Theme.Theme theme = RenderContext.Current.Theme;
        Font font = theme.GetFont(FontRole.Body);
        int pixelSize = Mathf.RoundToInt(new Rem(0.875f).ToFontPx());
        GUIStyle style = GuiStyleCache.GetOrCreate(font, pixelSize, FontStyle.Normal);
        style.alignment = TextAnchor.MiddleLeft;
        style.clipping = TextClipping.Overflow;

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

        int trackedAdvance = Mathf.Max(1, Mathf.RoundToInt(pixelSize * 0.04f));
        int[] charWidths = new int[upper.Length];
        int totalW = 0;
        for (int i = 0; i < upper.Length; i++) {
            GUIContent ch = new GUIContent(upper[i].ToString());
            charWidths[i] = Mathf.CeilToInt(style.CalcSize(ch).x);
            totalW += charWidths[i];
            if (i < upper.Length - 1) {
                totalW += trackedAdvance;
            }
        }

        int startX = Mathf.FloorToInt(labelRect.x + (labelRect.width - totalW) * 0.5f);
        int y = Mathf.FloorToInt(labelRect.y);
        int h = Mathf.CeilToInt(labelRect.height);
        int cursor = startX;
        for (int i = 0; i < upper.Length; i++) {
            string ch = upper[i].ToString();
            GUI.Label(new Rect(cursor, y, charWidths[i], h), ch, style);
            cursor += charWidths[i] + trackedAdvance;
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
        int pixelSize = Mathf.RoundToInt(new Rem(0.9f).ToFontPx());
        GUIStyle style = GuiStyleCache.GetOrCreate(font, pixelSize, FontStyle.Normal);
        style.alignment = TextAnchor.MiddleLeft;
        style.clipping = TextClipping.Overflow;

        string label = "[" + key.ToUpperInvariant() + "]";
        GUIContent gc = new GUIContent(label);
        float w = style.CalcSize(gc).x;
        float h = new Rem(1.25f).ToPixels();

        float x = tile.x + (tile.width - w) * 0.5f;
        float y = tile.yMax - h - new Rem(0.4f).ToPixels();
        Rect hint = RectSnap.SnapText(new Rect(x, y, w, h));

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

    

    private static void DrawChevron(Rect tile, bool disabled, float flipRatio) {
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

        float chevronSize = new Rem(1.0f).ToPixels();
        float cx = tile.x + tile.width * 0.5f;
        float yTop = tile.yMax - chevronSize - new Rem(0.525f).ToPixels();
        Rect chevronRect = RectSnap.Snap(new Rect(cx - chevronSize * 0.5f, yTop, chevronSize, chevronSize));

        float scaleY = 1f - 2f * flipRatio;
        Matrix4x4 savedMatrix = GUI.matrix;
        GUIUtility.ScaleAroundPivot(new Vector2(1f, scaleY), chevronRect.center);

        Color saved = GUI.color;
        GUI.color = tint;
        Texture2D tex = ChevronTextureCache.Down(strokePx: 38f, armSpan: 0.7f, armHeight: 0.42f);
        GUI.DrawTexture(chevronRect, tex);
        GUI.color = saved;
        GUI.matrix = savedMatrix;
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
