using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using Verse.Sound;
using static Cosmere.Lightweave.Hooks.Hooks;

namespace Cosmere.Lightweave.Input;

[Doc(
    Id = "checkbox",
    Summary = "Two-state toggle with adjacent label.",
    WhenToUse = "Capture a single boolean choice in a form or row.",
    SourcePath = "Lightweave/Lightweave/Input/Checkbox.cs"
)]
public static class Checkbox {
    public static LightweaveNode Create(
        [DocParam("Text rendered next to the box.")]
        string label,
        [DocParam("Current checked state.")]
        bool value,
        [DocParam("Invoked with the new value when toggled.")]
        Action<bool> onChange,
        [DocParam("Disables interaction and applies disabled styling.")]
        bool disabled = false,
        [DocParam("Optional translation key shown as a tooltip on hover.")]
        string? tooltipKey = null,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New($"Checkbox:{label}", line, file);
        node.ApplyStyling("checkbox", style, classes, id);
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
            }
            else if (mouseOver) {
                CursorOverrides.MarkDisabledHover();
            }

            if (!string.IsNullOrEmpty(tooltipKey)) {
                TooltipHandler.TipRegion(hitRect, (string)tooltipKey.Translate());
            }

            InteractionState boxState = new InteractionState(hovered, false, false, disabled);
            ThemeSlot borderSlot = InputSurface.ResolveToggleBorderSlot(boxState, value);

            BackgroundSpec boxBg = value
                ? BackgroundSpec.Of(disabled ? ThemeSlot.SurfaceDisabled : ThemeSlot.SurfaceAccent)
                : BackgroundSpec.Of(disabled ? ThemeSlot.SurfaceDisabled : ThemeSlot.SurfaceInput);
            BorderSpec boxBorder = BorderSpec.All(new Rem(2f / 16f), borderSlot);
            RadiusSpec boxRadius = RadiusSpec.All(RadiusScale.Xs);

            PaintBox.Draw(boxRect, boxBg, boxBorder, boxRadius);

            if (value) {
                Color savedCheck = GUI.color;
                GUI.color = theme.GetColor(ThemeSlot.TextOnAccent);
                DrawCheckmark(boxRect);
                GUI.color = savedCheck;
            }

            Font labelFont = theme.GetFont(FontRole.Body);
            int labelPixelSize = Mathf.RoundToInt(new Rem(1f).ToFontPx());
            GUIStyle labelStyle = GuiStyleCache.GetOrCreate(labelFont, labelPixelSize);
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

    [DocVariant("CL_Playground_Label_True")]
    public static DocSample DocsTrue() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<bool> s = UseState(true);
        return new DocSample(() => Create("Enabled", s.Value, v => s.Set(v), forced));
    }

    [DocVariant("CL_Playground_Label_False")]
    public static DocSample DocsFalse() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<bool> s = UseState(false);
        return new DocSample(() => Create("Disabled", s.Value, v => s.Set(v), forced));
    }

    [DocState("CL_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<bool> s = UseState(true);
        return new DocSample(() => Create("Default", s.Value, v => s.Set(v), forced));
    }

    [DocState("CL_Playground_Label_Hover")]
    public static DocSample DocsHover() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<bool> s = UseState(false);
        return new DocSample(() => Create("Hover", s.Value, v => s.Set(v), forced));
    }

    [DocState("CL_Playground_Label_Disabled")]
    public static DocSample DocsDisabled() {
        StateHandle<bool> s = UseState(true);
        return new DocSample(() => Create("Disabled", s.Value, v => s.Set(v), true));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        StateHandle<bool> s = UseState(true);
        return new DocSample(() => Create("Enabled", s.Value, v => s.Set(v)));
    }
}