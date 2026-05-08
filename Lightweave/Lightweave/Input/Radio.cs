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

public sealed class RadioGroupContext<T> {
    public RadioGroupContext(T value, Action<T> onChange) {
        Value = value;
        OnChange = onChange;
    }

    public T Value { get; }
    public Action<T> OnChange { get; }
}

[Doc(
    Id = "radio",
    Summary = "Mutually-exclusive set of options bound to a shared value.",
    WhenToUse = "Pick one of a small, fixed list of choices.",
    SourcePath = "Lightweave/Lightweave/Input/Radio.cs"
)]
public static class Radio {
    public static LightweaveNode Group<T>(
        [DocParam("Currently selected value.")]
        T value,
        [DocParam("Invoked with the new value when an item is selected.")]
        Action<T> onChange,
        [DocParam("Builder that adds Radio.Item children to the group.")]
        Action<List<LightweaveNode>> children,
        [DocParam("Vertical gap between items.", TypeOverride = "Rem", DefaultOverride = "0")]
        Rem? gap = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New($"RadioGroup<{typeof(T).Name}>", line, file);
        RadioGroupContext<T> ctx = new RadioGroupContext<T>(value, onChange);
        List<LightweaveNode> kids = new List<LightweaveNode>();

        RenderContext.Current.ContextValues.Push(ctx);
        try {
            children(kids);
        }
        finally {
            RenderContext.Current.ContextValues.Pop();
        }

        node.Children.AddRange(kids);

        Rem resolvedGap = gap ?? new Rem(0f);
        float rowHeight = new Rem(1.75f).ToPixels();
        float gapPx = resolvedGap.ToPixels();
        int childCount = kids.Count;
        if (childCount > 0) {
            node.PreferredHeight = rowHeight * childCount + gapPx * Math.Max(0, childCount - 1);
        }

        node.Paint = (rect, paintChildren) => {
            float y = rect.y;
            for (int i = 0; i < kids.Count; i++) {
                kids[i].MeasuredRect = new Rect(rect.x, y, rect.width, rowHeight);
                y += rowHeight + gapPx;
            }

            paintChildren();
        };
        return node;
    }

    public static LightweaveNode Item<T>(
        [DocParam("Label rendered next to the radio circle.")]
        string label,
        [DocParam("Value submitted to the group when this item is selected.")]
        T value,
        [DocParam("Disables interaction and applies disabled styling.")]
        bool disabled = false,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        // Context is resolved at construction because the group push/pop happens during
        // children() invocation; during Paint the stack no longer has the context.
        RadioGroupContext<T> group = Hooks.Hooks.UseContext<RadioGroupContext<T>>();
        LightweaveNode node = NodeBuilder.New($"Radio:{label}", line, file);
        node.PreferredHeight = new Rem(1.75f).ToPixels();

        node.Paint = (rect, paintChildren) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;
            bool selected = EqualityComparer<T>.Default.Equals(group.Value, value);

            float circleSize = new Rem(1.25f).ToPixels();
            float rowHeight = new Rem(1.75f).ToPixels();
            float gapPx = new Rem(0.5f).ToPixels();
            float rowY = rect.y;
            float circleY = rowY + (rowHeight - circleSize) / 2f;

            float circleX = rtl ? rect.xMax - circleSize : rect.x;
            Rect circleRect = new Rect(circleX, circleY, circleSize, circleSize);

            float labelX = rtl ? rect.x : circleX + circleSize + gapPx;
            float labelWidth = rtl
                ? circleX - gapPx - rect.x
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

            InteractionState circleState = new InteractionState(hovered, false, false, disabled);
            ThemeSlot borderSlot = InputSurface.ResolveToggleBorderSlot(circleState, selected);

            BackgroundSpec circleBg =
                BackgroundSpec.Of(disabled ? ThemeSlot.SurfaceDisabled : ThemeSlot.SurfaceInput);
            BorderSpec circleBorder = BorderSpec.All(new Rem(2f / 16f), borderSlot);
            RadiusSpec circleRadius = RadiusSpec.All(new Rem(0.625f));

            PaintBox.Draw(circleRect, circleBg, circleBorder, circleRadius);

            if (selected) {
                float dotSize = new Rem(0.5f).ToPixels();
                Rect dotRect = new Rect(
                    circleRect.x + (circleSize - dotSize) / 2f,
                    circleRect.y + (circleSize - dotSize) / 2f,
                    dotSize,
                    dotSize
                );
                ThemeSlot dotSlot = disabled ? ThemeSlot.BorderOff : ThemeSlot.SurfaceAccent;
                BackgroundSpec dotBg = BackgroundSpec.Of(dotSlot);
                RadiusSpec dotRadius = RadiusSpec.All(new Rem(0.25f));
                PaintBox.Draw(dotRect, dotBg, null, dotRadius);
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
                group.OnChange?.Invoke(value);
                e.Use();
            }
        };

        return node;
    }

    [DocVariant("CC_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<int> s = UseState(1);
        return new DocSample(() => Group<int>(
            s.Value,
            v => s.Set(v),
            k => {
                k.Add(Item((string)"CC_Playground_Controls_Radio_OptionA".Translate(), 0, forced));
                k.Add(Item((string)"CC_Playground_Controls_Radio_OptionB".Translate(), 1, forced));
                k.Add(Item((string)"CC_Playground_Controls_Radio_OptionC".Translate(), 2, forced));
            }
        ));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        StateHandle<int> s = UseState(0);
        return new DocSample(() => Group<int>(
            s.Value,
            v => s.Set(v),
            k => {
                k.Add(Item("Option A", 0));
                k.Add(Item("Option B", 1));
            }
        ));
    }
}