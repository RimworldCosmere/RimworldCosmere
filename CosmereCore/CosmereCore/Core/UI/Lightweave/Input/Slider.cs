using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Hooks;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Input;

public static class Slider
{
    public static LightweaveNode Create(
        float value,
        Action<float> onChange,
        float min = 0f,
        float max = 1f,
        float step = 0f,
        float[]? marks = null,
        Func<float, string>? format = null,
        bool disabled = false,
        [CallerFilePath] string? caller = null,
        [CallerLineNumber] int line = 0)
    {
        LightweaveNode node = NodeBuilder.New("Slider", line, caller ?? string.Empty);

        node.Paint = (rect, paintChildren) =>
        {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            Hooks.Hooks.RefHandle<bool> dragging = Hooks.Hooks.UseRef<bool>(false);

            float labelBandHeight = new Rem(1f).ToPixels();
            float trackBandHeight = new Rem(1.25f).ToPixels();
            float trackThickness = new Rem(0.5f).ToPixels();
            float thumbSize = new Rem(1f).ToPixels();
            float tickWidth = new Rem(1f / 16f).ToPixels();
            float tickHeight = new Rem(0.75f).ToPixels();
            float edgePadding = thumbSize / 2f;

            Rect labelBand = new Rect(rect.x, rect.y, rect.width, labelBandHeight);
            Rect trackBand = new Rect(rect.x, rect.y + labelBandHeight, rect.width, trackBandHeight);

            float trackLeft = trackBand.x + edgePadding;
            float trackRight = trackBand.xMax - edgePadding;
            float trackWidth = Mathf.Max(0f, trackRight - trackLeft);
            float trackY = trackBand.y + (trackBand.height - trackThickness) / 2f;
            Rect trackRect = new Rect(trackLeft, trackY, trackWidth, trackThickness);

            float range = max - min;
            float clampedValue = Mathf.Clamp(value, min, max);
            float logicalFraction = range > 0f ? (clampedValue - min) / range : 0f;
            float physicalFraction = rtl ? 1f - logicalFraction : logicalFraction;
            float thumbCenterX = trackRect.x + physicalFraction * trackRect.width;
            float thumbX = thumbCenterX - thumbSize / 2f;
            float thumbY = trackBand.y + (trackBand.height - thumbSize) / 2f;
            Rect thumbRect = new Rect(thumbX, thumbY, thumbSize, thumbSize);

            ThemeSlot filledSlot = disabled ? ThemeSlot.SurfaceDisabled : ThemeSlot.SurfaceAccent;
            ThemeSlot unfilledSlot = disabled ? ThemeSlot.SurfaceDisabled : ThemeSlot.SurfaceInput;
            RadiusSpec trackRadius = RadiusSpec.All(new Rem(0.25f));

            if (rtl)
            {
                Rect rightUnfilled = new Rect(trackRect.x, trackRect.y, Mathf.Max(0f, thumbCenterX - trackRect.x), trackRect.height);
                Rect leftFilled = new Rect(thumbCenterX, trackRect.y, Mathf.Max(0f, trackRect.xMax - thumbCenterX), trackRect.height);
                PaintBox.Draw(rightUnfilled, new BackgroundSpec.Solid(unfilledSlot), null, trackRadius);
                PaintBox.Draw(leftFilled, new BackgroundSpec.Solid(filledSlot), null, trackRadius);
            }
            else
            {
                Rect leftFilled = new Rect(trackRect.x, trackRect.y, Mathf.Max(0f, thumbCenterX - trackRect.x), trackRect.height);
                Rect rightUnfilled = new Rect(thumbCenterX, trackRect.y, Mathf.Max(0f, trackRect.xMax - thumbCenterX), trackRect.height);
                PaintBox.Draw(leftFilled, new BackgroundSpec.Solid(filledSlot), null, trackRadius);
                PaintBox.Draw(rightUnfilled, new BackgroundSpec.Solid(unfilledSlot), null, trackRadius);
            }

            if (marks != null && marks.Length > 0 && range > 0f)
            {
                ThemeSlot markSlot = disabled ? ThemeSlot.BorderSubtle : ThemeSlot.BorderDefault;
                BackgroundSpec markBg = new BackgroundSpec.Solid(markSlot);
                float markY = trackBand.y + (trackBand.height - tickHeight) / 2f;
                for (int i = 0; i < marks.Length; i++)
                {
                    float markValue = Mathf.Clamp(marks[i], min, max);
                    float markLogical = (markValue - min) / range;
                    float markPhysical = rtl ? 1f - markLogical : markLogical;
                    float markX = trackRect.x + markPhysical * trackRect.width - tickWidth / 2f;
                    Rect markRect = new Rect(markX, markY, tickWidth, tickHeight);
                    PaintBox.Draw(markRect, markBg, null, null);
                }
            }

            InteractionState thumbState = InteractionState.Resolve(thumbRect, focusName: null, disabled: disabled);
            ThemeSlot thumbFillSlot = disabled
                ? ThemeSlot.SurfaceDisabled
                : ThemeSlot.SurfaceAccent;
            ThemeSlot thumbBorderSlot = disabled
                ? ThemeSlot.BorderSubtle
                : thumbState.Hovered || dragging.Current
                    ? ThemeSlot.BorderHover
                    : ThemeSlot.BorderDefault;
            BackgroundSpec thumbBg = new BackgroundSpec.Solid(thumbFillSlot);
            BorderSpec thumbBorder = BorderSpec.All(new Rem(1f / 16f), thumbBorderSlot);
            RadiusSpec thumbRadius = RadiusSpec.All(new Rem(0.5f));
            PaintBox.Draw(thumbRect, thumbBg, thumbBorder, thumbRadius);

            if (!disabled && (thumbState.Hovered || dragging.Current))
            {
                Color overlayColor = new Color(1f, 1f, 1f, 0.08f);
                PaintBox.Draw(thumbRect, new BackgroundSpec.Solid(overlayColor), null, thumbRadius);
            }

            string labelText = format != null ? format(clampedValue) : $"{clampedValue:0.00}";
            Font labelFont = theme.GetFont(FontRole.Caption);
            int labelPixelSize = Mathf.RoundToInt(new Rem(0.75f).ToPixels());
            GUIStyle labelStyle = GuiStyleCache.Get(labelFont, labelPixelSize, FontStyle.Normal);
            labelStyle.alignment = rtl ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
            Color labelColor = disabled
                ? theme.GetColor(ThemeSlot.TextMuted)
                : theme.GetColor(ThemeSlot.TextPrimary);
            Color savedLabel = GUI.color;
            GUI.color = labelColor;
            GUI.Label(RectSnap.Snap(labelBand), labelText, labelStyle);
            GUI.color = savedLabel;

            paintChildren();

            if (disabled)
            {
                return;
            }

            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && trackBand.Contains(e.mousePosition))
            {
                dragging.Current = true;
                UpdateValue(e.mousePosition.x, trackRect, min, max, step, rtl, value, onChange);
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && dragging.Current)
            {
                UpdateValue(e.mousePosition.x, trackRect, min, max, step, rtl, value, onChange);
                e.Use();
            }
            else if ((e.type == EventType.MouseUp || e.rawType == EventType.MouseUp) && dragging.Current)
            {
                dragging.Current = false;
                if (e.type == EventType.MouseUp)
                {
                    e.Use();
                }
            }
        };

        return node;
    }

    private static void UpdateValue(
        float mouseX,
        Rect trackRect,
        float min,
        float max,
        float step,
        bool rtl,
        float currentValue,
        Action<float> onChange)
    {
        if (trackRect.width <= 0f)
        {
            return;
        }
        float localX = mouseX - trackRect.x;
        float fraction = Mathf.Clamp01(localX / trackRect.width);
        if (rtl)
        {
            fraction = 1f - fraction;
        }
        float newValue = Mathf.Lerp(min, max, fraction);
        if (step > 0f)
        {
            newValue = min + Mathf.Round((newValue - min) / step) * step;
            newValue = Mathf.Clamp(newValue, min, max);
        }
        if (!Mathf.Approximately(newValue, currentValue))
        {
            onChange?.Invoke(newValue);
        }
    }
}
