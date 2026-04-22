using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Hooks;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Input;

public sealed class RadioGroupContext<T>
{
    public T Value { get; }
    public Action<T> OnChange { get; }

    public RadioGroupContext(T value, Action<T> onChange)
    {
        Value = value;
        OnChange = onChange;
    }
}

public static class Radio
{
    public static LightweaveNode Group<T>(
        T value,
        Action<T> onChange,
        Action<List<LightweaveNode>> children,
        Rem? gap = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        LightweaveNode node = NodeBuilder.New($"RadioGroup<{typeof(T).Name}>", line, file);
        RadioGroupContext<T> ctx = new RadioGroupContext<T>(value, onChange);
        List<LightweaveNode> kids = new List<LightweaveNode>();

        RenderContext.Current.ContextValues.Push(ctx);
        try
        {
            children(kids);
        }
        finally
        {
            RenderContext.Current.ContextValues.Pop();
        }

        node.Children.AddRange(kids);

        Rem resolvedGap = gap ?? new Rem(0f);
        node.Paint = (rect, paintChildren) =>
        {
            float rowHeight = new Rem(1.75f).ToPixels();
            float gapPx = resolvedGap.ToPixels();
            float y = rect.y;
            for (int i = 0; i < kids.Count; i++)
            {
                kids[i].MeasuredRect = new Rect(rect.x, y, rect.width, rowHeight);
                y += rowHeight + gapPx;
            }
            paintChildren();
        };
        return node;
    }

    public static LightweaveNode Item<T>(
        string label,
        T value,
        bool disabled = false,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        // Context is resolved at construction because the group push/pop happens during
        // children() invocation; during Paint the stack no longer has the context.
        RadioGroupContext<T> group = Hooks.Hooks.UseContext<RadioGroupContext<T>>();
        LightweaveNode node = NodeBuilder.New($"Radio:{label}", line, file);

        node.Paint = (rect, paintChildren) =>
        {
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

            BackgroundSpec circleBg = new BackgroundSpec.Solid(disabled ? ThemeSlot.SurfaceDisabled : ThemeSlot.SurfaceInput);
            ThemeSlot borderSlot = disabled ? ThemeSlot.BorderSubtle : ThemeSlot.BorderDefault;
            BorderSpec circleBorder = BorderSpec.All(new Rem(1f / 16f), borderSlot);
            RadiusSpec circleRadius = RadiusSpec.All(new Rem(0.625f));

            PaintBox.Draw(circleRect, circleBg, circleBorder, circleRadius);

            if (selected)
            {
                float dotSize = new Rem(0.5f).ToPixels();
                Rect dotRect = new Rect(
                    circleRect.x + (circleSize - dotSize) / 2f,
                    circleRect.y + (circleSize - dotSize) / 2f,
                    dotSize,
                    dotSize);
                BackgroundSpec dotBg = new BackgroundSpec.Solid(disabled ? ThemeSlot.TextMuted : ThemeSlot.SurfaceAccent);
                RadiusSpec dotRadius = RadiusSpec.All(new Rem(0.25f));
                PaintBox.Draw(dotRect, dotBg, null, dotRadius);
            }

            Font labelFont = theme.GetFont(FontRole.Body);
            int labelPixelSize = Mathf.RoundToInt(new Rem(1f).ToPixels());
            GUIStyle labelStyle = GuiStyleCache.Get(labelFont, labelPixelSize, FontStyle.Normal);
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
            if (!disabled && e.type == EventType.MouseUp && e.button == 0 && hitRect.Contains(e.mousePosition))
            {
                group.OnChange?.Invoke(value);
                e.Use();
            }
        };

        return node;
    }
}
