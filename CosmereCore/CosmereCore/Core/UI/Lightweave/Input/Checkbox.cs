using System;
using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.UI.Lightweave.Input;

public static class Checkbox {
    public static LightweaveNode Create(
        string label,
        bool value,
        Action<bool> onChange,
        bool disabled = false,
        string? tooltipKey = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New($"Checkbox:{label}", line, file);
        node.PreferredHeight = new Rem(1.75f).ToPixels();

        node.Paint = (rect, paintChildren) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            float boxSize = new Rem(1.25f).ToPixels();
            float rowHeight = new Rem(1.75f).ToPixels();
            float gapPx = new Rem(0.5f).ToPixels();
            float rowY = rect.y + Mathf.Max(0f, (Mathf.Min(rect.height, rowHeight) - rowHeight) / 2f);
            float boxY = rowY + (rowHeight - boxSize) / 2f;

            float boxX = rtl ? rect.xMax - boxSize : rect.x;
            Rect boxRect = new Rect(boxX, boxY, boxSize, boxSize);

            float labelX = rtl ? rect.x : boxX + boxSize + gapPx;
            float labelWidth = rtl
                ? boxX - gapPx - rect.x
                : rect.xMax - labelX;
            Rect labelRect = new Rect(labelX, rowY, Mathf.Max(0f, labelWidth), rowHeight);

            Rect hitRect = new Rect(rect.x, rowY, rect.width, rowHeight);
            LightweaveHitTracker.Track(hitRect);

            bool mouseOver = Mouse.IsOver(hitRect);
            bool hovered = !disabled && mouseOver;
            if (!disabled) {
                MouseoverSounds.DoRegion(hitRect);
            } else if (mouseOver) {
                CursorOverrides.MarkDisabledHover();
            }

            if (!string.IsNullOrEmpty(tooltipKey)) {
                TooltipHandler.TipRegion(hitRect, (string)tooltipKey.Translate());
            }

            InteractionState boxState = new InteractionState(hovered, false, false, disabled);
            ThemeSlot borderSlot = InputSurface.ResolveToggleBorderSlot(boxState, value);

            BackgroundSpec boxBg = value
                ? new BackgroundSpec.Solid(disabled ? ThemeSlot.SurfaceDisabled : ThemeSlot.SurfaceAccent)
                : new BackgroundSpec.Solid(disabled ? ThemeSlot.SurfaceDisabled : ThemeSlot.SurfaceInput);
            BorderSpec boxBorder = BorderSpec.All(new Rem(2f / 16f), borderSlot);
            RadiusSpec boxRadius = RadiusSpec.All(new Rem(0.125f));

            PaintBox.Draw(boxRect, boxBg, boxBorder, boxRadius);

            if (value) {
                Color savedCheck = GUI.color;
                GUI.color = theme.GetColor(ThemeSlot.TextOnAccent);
                DrawCheckmark(boxRect);
                GUI.color = savedCheck;
            }

            Font labelFont = theme.GetFont(FontRole.Body);
            int labelPixelSize = Mathf.RoundToInt(new Rem(1f).ToFontPx());
            GUIStyle labelStyle = GuiStyleCache.Get(labelFont, labelPixelSize);
            labelStyle.alignment = rtl ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
            Color labelColor = disabled
                ? theme.GetColor(ThemeSlot.TextMuted)
                : theme.GetColor(ThemeSlot.TextPrimary);
            Color savedLabel = GUI.color;
            GUI.color = labelColor;
            GUI.Label(RectSnap.Snap(labelRect), label, labelStyle);
            GUI.color = savedLabel;

            paintChildren();

            Event e = Event.current;
            if (!disabled && e.type == EventType.MouseUp && e.button == 0 && hitRect.Contains(e.mousePosition)) {
                onChange?.Invoke(!value);
                e.Use();
            }
        };

        return node;
    }

    private static void DrawCheckmark(Rect rect) {
        float pad = rect.width * 0.18f;
        float stroke = Mathf.Max(2f, rect.width * 0.18f);
        Vector2 p1 = new Vector2(rect.x + pad, rect.y + rect.height * 0.52f);
        Vector2 p2 = new Vector2(rect.x + rect.width * 0.42f, rect.yMax - pad);
        Vector2 p3 = new Vector2(rect.xMax - pad, rect.y + pad);
        DrawLine(p1, p2, stroke);
        DrawLine(p2, p3, stroke);
    }

    private static void DrawLine(Vector2 a, Vector2 b, float thickness) {
        Vector2 delta = b - a;
        float length = delta.magnitude;
        if (length <= 0.001f) {
            return;
        }

        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        Matrix4x4 saved = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, a);
        GUI.DrawTexture(new Rect(a.x, a.y - thickness * 0.5f, length, thickness), Texture2D.whiteTexture);
        GUI.matrix = saved;
    }
}