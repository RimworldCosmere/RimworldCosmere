using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.MainMenu;

public static class FootLink {
    public static LightweaveNode Create(
        string label,
        Action onClick,
        bool indicateMenu = false,
        bool expanded = false,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Hooks.Hooks.RefHandle<float> flipRatio = Hooks.Hooks.UseRef<float>(expanded ? 1f : 0f, line, file + "#foot-flip");

        LightweaveNode node = NodeBuilder.New($"FootLink:{label}", line, file);
        node.ApplyStyling("foot-link", style, classes, id);
        node.PreferredHeight = new Rem(2f).ToPixels();
        node.MeasureWidth = () => MeasureWidth(label, indicateMenu);

        node.Paint = (rect, _) => {
            InteractionState state = InteractionState.Resolve(rect, null, false);
            bool active = state.Hovered || state.Pressed;
            Theme.Theme theme = RenderContext.Current.Theme;
            ThemeSlot slot = active ? ThemeSlot.SurfaceAccent : ThemeSlot.TextMuted;
            Color color = theme.GetColor(slot);

            string upper = (label ?? string.Empty).ToUpperInvariant();

            Font font = theme.GetFont(FontRole.Mono);
            int pixelSize = Mathf.RoundToInt(new Rem(0.75f).ToFontPx());
            GUIStyle style = GuiStyleCache.GetOrCreate(font, pixelSize, FontStyle.Normal);
            style.alignment = TextAnchor.MiddleLeft;

            float tracking = pixelSize * 0.18f;
            float chevronGap = pixelSize * 0.6f;
            float chevronW = indicateMenu ? pixelSize * 0.8f : 0f;

            float labelW = 0f;
            for (int i = 0; i < upper.Length; i++) {
                GUIContent gc = new GUIContent(upper[i].ToString());
                labelW += style.CalcSize(gc).x;
                if (i < upper.Length - 1) {
                    labelW += tracking;
                }
            }

            float totalW = labelW + (indicateMenu ? chevronGap + chevronW : 0f);
            float startX = rect.x + (rect.width - totalW) * 0.5f;

            Color saved = GUI.color;
            GUI.color = color;
            float cursor = startX;
            for (int i = 0; i < upper.Length; i++) {
                string ch = upper[i].ToString();
                GUIContent gc = new GUIContent(ch);
                float w = style.CalcSize(gc).x;
                GUI.Label(RectSnap.Snap(new Rect(cursor, rect.y, w, rect.height)), ch, style);
                cursor += w + tracking;
            }

            if (indicateMenu) {
                float target = expanded ? 1f : 0f;
                float dt = Time.unscaledDeltaTime;
                flipRatio.Current = Mathf.MoveTowards(flipRatio.Current, target, dt / 0.16f);

                float cx = startX + labelW + chevronGap + chevronW * 0.5f;
                float cy = rect.y + rect.height * 0.5f;
                DrawChevron(cx, cy, chevronW, color, flipRatio.Current);
            }
            GUI.color = saved;

            InteractionFeedback.Apply(rect, true, true);
            node.MeasuredRect = rect;

            Event e = Event.current;
            if (e.type == EventType.MouseUp && e.button == 0 && rect.Contains(e.mousePosition)) {
                onClick?.Invoke();
                e.Use();
            }
        };
        return node;
    }

    private static float MeasureWidth(string label, bool indicateMenu) {
        Theme.Theme theme = RenderContext.Current.Theme;
        Font font = theme.GetFont(FontRole.Mono);
        int pixelSize = Mathf.RoundToInt(new Rem(0.75f).ToFontPx());
        GUIStyle style = GuiStyleCache.GetOrCreate(font, pixelSize, FontStyle.Normal);
        string upper = (label ?? string.Empty).ToUpperInvariant();
        float tracking = pixelSize * 0.18f;
        float w = 0f;
        for (int i = 0; i < upper.Length; i++) {
            w += style.CalcSize(new GUIContent(upper[i].ToString())).x;
            if (i < upper.Length - 1) {
                w += tracking;
            }
        }
        if (indicateMenu) {
            w += pixelSize * 0.6f + pixelSize * 0.8f;
        }
        return w + new Rem(1f).ToPixels();
    }


    private static void DrawChevron(float cx, float cy, float size, Color color, float flipRatio) {
        if (Event.current.type != EventType.Repaint) {
            return;
        }

        float boxSize = size * 1.4f;
        Rect chevronRect = RectSnap.Snap(new Rect(cx - boxSize * 0.5f, cy - boxSize * 0.5f, boxSize, boxSize));

        float scaleY = 1f - 2f * flipRatio;
        Matrix4x4 savedMatrix = GUI.matrix;
        GUIUtility.ScaleAroundPivot(new Vector2(1f, scaleY), chevronRect.center);

        Color saved = GUI.color;
        GUI.color = color;
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
        GUI.DrawTexture(RectSnap.Snap(new Rect(x0, y0 - thickness * 0.5f, len, thickness)), BaseContent.WhiteTex);
        GUI.matrix = prev;
        GUI.color = saved;
    }
}
