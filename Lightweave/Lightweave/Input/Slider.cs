using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Lightweave.Input;

[Doc(
    Id = "slider",
    Summary = "Continuous or stepped value selector along a horizontal track.",
    WhenToUse = "Pick a numeric value within a known range.",
    SourcePath = "Lightweave/Lightweave/Input/Slider.cs"
)]
public static class Slider {
    public static LightweaveNode Create(
        [DocParam("Current slider value.")]
        float value,
        [DocParam("Invoked with the new value while dragging or after a click.")]
        Action<float> onChange,
        [DocParam("Minimum value.")]
        float min = 0f,
        [DocParam("Maximum value.")]
        float max = 1f,
        [DocParam("Snap interval. 0 disables stepping.")]
        float step = 0f,
        [DocParam("Optional list of tick marks rendered along the track.")]
        float[]? marks = null,
        [DocParam("Optional formatter used to render the value label.")]
        Func<float, string>? format = null,
        [DocParam("Disables interaction and applies disabled styling.")]
        bool disabled = false,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("Slider", line, file);
        node.PreferredHeight = new Rem(2.25f).ToPixels();

        node.Paint = (rect, paintChildren) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            Hooks.Hooks.RefHandle<bool> dragging = Hooks.Hooks.UseRef(false, line, file);

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

            if (rtl) {
                Rect rightUnfilled = new Rect(
                    trackRect.x,
                    trackRect.y,
                    Mathf.Max(0f, thumbCenterX - trackRect.x),
                    trackRect.height
                );
                Rect leftFilled = new Rect(
                    thumbCenterX,
                    trackRect.y,
                    Mathf.Max(0f, trackRect.xMax - thumbCenterX),
                    trackRect.height
                );
                PaintBox.Draw(rightUnfilled, new BackgroundSpec.Solid(unfilledSlot), null, trackRadius);
                PaintBox.Draw(leftFilled, new BackgroundSpec.Solid(filledSlot), null, trackRadius);
            } else {
                Rect leftFilled = new Rect(
                    trackRect.x,
                    trackRect.y,
                    Mathf.Max(0f, thumbCenterX - trackRect.x),
                    trackRect.height
                );
                Rect rightUnfilled = new Rect(
                    thumbCenterX,
                    trackRect.y,
                    Mathf.Max(0f, trackRect.xMax - thumbCenterX),
                    trackRect.height
                );
                PaintBox.Draw(leftFilled, new BackgroundSpec.Solid(filledSlot), null, trackRadius);
                PaintBox.Draw(rightUnfilled, new BackgroundSpec.Solid(unfilledSlot), null, trackRadius);
            }

            if (marks != null && marks.Length > 0 && range > 0f) {
                ThemeSlot markSlot = disabled ? ThemeSlot.BorderSubtle : ThemeSlot.BorderDefault;
                BackgroundSpec markBg = new BackgroundSpec.Solid(markSlot);
                float markY = trackBand.y + (trackBand.height - tickHeight) / 2f;
                for (int i = 0; i < marks.Length; i++) {
                    float markValue = Mathf.Clamp(marks[i], min, max);
                    float markLogical = (markValue - min) / range;
                    float markPhysical = rtl ? 1f - markLogical : markLogical;
                    float markX = trackRect.x + markPhysical * trackRect.width - tickWidth / 2f;
                    Rect markRect = new Rect(markX, markY, tickWidth, tickHeight);
                    PaintBox.Draw(markRect, markBg, null, null);
                }
            }

            InteractionState thumbState = InteractionState.Resolve(thumbRect, null, disabled);
            RadiusSpec thumbRadius = RadiusSpec.All(new Rem(0.5f));

            if (!disabled && (thumbState.Hovered || dragging.Current)) {
                float ringGrowPx = new Rem(0.25f).ToPixels();
                Rect ringRect = new Rect(
                    thumbRect.x - ringGrowPx,
                    thumbRect.y - ringGrowPx,
                    thumbRect.width + ringGrowPx * 2f,
                    thumbRect.height + ringGrowPx * 2f
                );
                Color ringColor = theme.GetColor(ThemeSlot.BorderFocus);
                ringColor.a = 0.30f;
                PaintBox.Draw(ringRect, new BackgroundSpec.Solid(ringColor), null, RadiusSpec.All(new Rem(0.625f)));
            }

            ThemeSlot thumbFillSlot = disabled
                ? ThemeSlot.SurfaceDisabled
                : ThemeSlot.TextOnAccent;
            ThemeSlot thumbBorderSlot = disabled
                ? ThemeSlot.BorderOff
                : thumbState.Hovered || dragging.Current
                    ? ThemeSlot.BorderFocus
                    : ThemeSlot.BorderDefault;
            BackgroundSpec thumbBg = new BackgroundSpec.Solid(thumbFillSlot);
            BorderSpec thumbBorder = BorderSpec.All(new Rem(2f / 16f), thumbBorderSlot);
            PaintBox.Draw(thumbRect, thumbBg, thumbBorder, thumbRadius);

            if (!disabled) {
                Rect thumbCore = new Rect(
                    thumbRect.x + thumbRect.width * 0.30f,
                    thumbRect.y + thumbRect.height * 0.30f,
                    thumbRect.width * 0.40f,
                    thumbRect.height * 0.40f
                );
                PaintBox.Draw(
                    thumbCore,
                    new BackgroundSpec.Solid(ThemeSlot.SurfaceAccent),
                    null,
                    RadiusSpec.All(new Rem(0.5f))
                );
            }

            string labelText = format != null ? format(clampedValue) : $"{clampedValue:0.00}";
            Font labelFont = theme.GetFont(FontRole.Caption);
            int labelPixelSize = Mathf.RoundToInt(new Rem(0.75f).ToFontPx());
            GUIStyle labelStyle = GuiStyleCache.Get(labelFont, labelPixelSize);
            labelStyle.alignment = rtl ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
            Color labelColor = disabled
                ? theme.GetColor(ThemeSlot.TextMuted)
                : theme.GetColor(ThemeSlot.TextPrimary);
            Color savedLabel = GUI.color;
            GUI.color = labelColor;
            GUI.Label(RectSnap.Snap(labelBand), labelText, labelStyle);
            GUI.color = savedLabel;

            paintChildren();

            if (disabled) {
                return;
            }

            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && trackBand.Contains(e.mousePosition)) {
                dragging.Current = true;
                UpdateValue(e.mousePosition.x, trackRect, min, max, step, rtl, value, onChange);
                e.Use();
            } else if (e.type == EventType.MouseDrag && dragging.Current) {
                UpdateValue(e.mousePosition.x, trackRect, min, max, step, rtl, value, onChange);
                e.Use();
            } else if ((e.type == EventType.MouseUp || e.rawType == EventType.MouseUp) && dragging.Current) {
                dragging.Current = false;
                if (e.type == EventType.MouseUp) {
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
        Action<float> onChange
    ) {
        if (trackRect.width <= 0f) {
            return;
        }

        float localX = mouseX - trackRect.x;
        float fraction = Mathf.Clamp01(localX / trackRect.width);
        if (rtl) {
            fraction = 1f - fraction;
        }

        float newValue = Mathf.Lerp(min, max, fraction);
        if (step > 0f) {
            newValue = min + Mathf.Round((newValue - min) / step) * step;
            newValue = Mathf.Clamp(newValue, min, max);
        }

        if (!Mathf.Approximately(newValue, currentValue)) {
            onChange?.Invoke(newValue);
        }
    }

    [DocVariant("CC_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(Create(0.4f, _ => { }, disabled: forced));
    }

    [DocVariant("CC_Playground_Label_Accented")]
    public static DocSample DocsAccented() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(Create(
            0.4f,
            _ => { },
            0f,
            1f,
            0.25f,
            new[] { 0f, 0.25f, 0.5f, 0.75f, 1f },
            disabled: forced
        ));
    }

    [DocState("CC_Playground_Label_Default")]
    public static DocSample DocsDefaultState() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(Create(0.4f, _ => { }, disabled: forced));
    }

    [DocState("CC_Playground_Label_Hover")]
    public static DocSample DocsHover() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(Create(0.7f, _ => { }, disabled: forced));
    }

    [DocState("CC_Playground_Label_Disabled")]
    public static DocSample DocsDisabled() {
        return new DocSample(Create(0.4f, _ => { }, disabled: true));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(Create(0.5f, _ => { }));
    }
}