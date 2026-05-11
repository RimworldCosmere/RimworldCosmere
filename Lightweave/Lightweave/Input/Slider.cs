using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using static Cosmere.Lightweave.Hooks.Hooks;

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
        [DocParam("Invoked with the new value. Fires on release by default; see `live`.")]
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
        [DocParam("When true, fires onChange during drag. When false (default), only on mouse-up.")]
        bool live = false,
        [DocParam("When `live` is true, minimum frames between onChange invocations during drag. Default 10.")]
        int liveThrottleFrames = 10,
        [DocParam("Minimum frames between label string re-formats during drag. Default 3 (~20Hz at 60fps).")]
        int labelThrottleFrames = 3,
        [DocParam("When false, the slider does not render its own readout label band (use this when the readout is rendered externally, e.g. by SliderWithReadout).")]
        bool showReadout = true,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("Slider", line, file);
        node.ApplyStyling("slider", style, classes, id);
        node.PreferredHeight = (showReadout ? new Rem(2.25f) : new Rem(1.25f)).ToPixels();

        node.Paint = (rect, paintChildren) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            Hooks.Hooks.RefHandle<bool> dragging = Hooks.Hooks.UseRef(false, line, file);
            Hooks.Hooks.RefHandle<float> draftValue = Hooks.Hooks.UseRef(value, line, file + "#draft");
            Hooks.Hooks.RefHandle<int> lastFireFrame = Hooks.Hooks.UseRef(int.MinValue, line, file + "#lastFire");
            Hooks.Hooks.RefHandle<string> cachedLabel = Hooks.Hooks.UseRef(string.Empty, line, file + "#labelCache");
            Hooks.Hooks.RefHandle<int> lastLabelFrame = Hooks.Hooks.UseRef(int.MinValue, line, file + "#labelFrame");
            Hooks.Hooks.RefHandle<float> lastLabelValue = Hooks.Hooks.UseRef(float.NaN, line, file + "#labelValue");

            float labelBandHeight = showReadout ? new Rem(1f).ToPixels() : 0f;
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

            if (!disabled && dragging.Current && trackWidth > 0f) {
                float computed = ComputeValue(Event.current.mousePosition.x, trackRect, min, max, step, rtl);
                if (!Mathf.Approximately(computed, draftValue.Current)) {
                    draftValue.Current = computed;
                }
            }

            float effectiveValue = dragging.Current ? draftValue.Current : value;
            float range = max - min;
            float clampedValue = Mathf.Clamp(effectiveValue, min, max);
            float logicalFraction = range > 0f ? (clampedValue - min) / range : 0f;
            float physicalFraction = rtl ? 1f - logicalFraction : logicalFraction;
            float thumbCenterX = trackRect.x + physicalFraction * trackRect.width;
            float thumbX = thumbCenterX - thumbSize / 2f;
            float thumbY = trackBand.y + (trackBand.height - thumbSize) / 2f;
            Rect thumbRect = new Rect(thumbX, thumbY, thumbSize, thumbSize);

            ThemeSlot filledSlot = disabled ? ThemeSlot.SurfaceDisabled : ThemeSlot.SurfaceAccent;
            ThemeSlot unfilledSlot = disabled ? ThemeSlot.SurfaceDisabled : ThemeSlot.SurfaceInput;
            RadiusSpec trackRadius = RadiusSpec.All(RadiusScale.Sm);

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
                PaintBox.Draw(rightUnfilled, BackgroundSpec.Of(unfilledSlot), null, trackRadius);
                PaintBox.Draw(leftFilled, BackgroundSpec.Of(filledSlot), null, trackRadius);
            }
            else {
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
                PaintBox.Draw(leftFilled, BackgroundSpec.Of(filledSlot), null, trackRadius);
                PaintBox.Draw(rightUnfilled, BackgroundSpec.Of(unfilledSlot), null, trackRadius);
            }

            if (marks != null && marks.Length > 0 && range > 0f) {
                ThemeSlot markSlot = disabled ? ThemeSlot.BorderSubtle : ThemeSlot.BorderDefault;
                BackgroundSpec markBg = BackgroundSpec.Of(markSlot);
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
            RadiusSpec thumbRadius = RadiusSpec.All(RadiusScale.Lg);

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
                PaintBox.Draw(ringRect, BackgroundSpec.Of(ringColor), null, RadiusSpec.All(RadiusScale.Xl));
            }

            ThemeSlot thumbFillSlot = disabled
                ? ThemeSlot.SurfaceDisabled
                : ThemeSlot.TextOnAccent;
            ThemeSlot thumbBorderSlot = disabled
                ? ThemeSlot.BorderOff
                : thumbState.Hovered || dragging.Current
                    ? ThemeSlot.BorderFocus
                    : ThemeSlot.BorderDefault;
            BackgroundSpec thumbBg = BackgroundSpec.Of(thumbFillSlot);
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
                    BackgroundSpec.Of(ThemeSlot.SurfaceAccent),
                    null,
                    RadiusSpec.All(RadiusScale.Lg)
                );
            }

            if (showReadout && Event.current.type == EventType.Repaint) {
                int currentFrame = Time.frameCount;
                int labelThrottle = Mathf.Max(0, labelThrottleFrames);
                bool valueChanged = !Mathf.Approximately(lastLabelValue.Current, clampedValue);
                bool throttleElapsed = currentFrame - lastLabelFrame.Current >= labelThrottle;
                bool stale = string.IsNullOrEmpty(cachedLabel.Current);
                if (stale || (valueChanged && (!dragging.Current || throttleElapsed))) {
                    cachedLabel.Current = format != null ? format(clampedValue) : $"{clampedValue:0.00}";
                    lastLabelValue.Current = clampedValue;
                    lastLabelFrame.Current = currentFrame;
                }

                Font labelFont = theme.GetFont(FontRole.Caption);
                int labelPixelSize = Mathf.RoundToInt(new Rem(0.75f).ToFontPx());
                GUIStyle labelStyle = GuiStyleCache.GetOrCreate(labelFont, labelPixelSize);
                labelStyle.alignment = rtl ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
                Color labelColor = disabled
                    ? theme.GetColor(ThemeSlot.TextMuted)
                    : theme.GetColor(ThemeSlot.TextPrimary);
                Color savedLabel = GUI.color;
                GUI.color = labelColor;
                GUI.Label(RectSnap.Snap(labelBand), cachedLabel.Current, labelStyle);
                GUI.color = savedLabel;
            }

            paintChildren();

            if (disabled) {
                return;
            }

            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && trackBand.Contains(e.mousePosition)) {
                dragging.Current = true;
                float computed = ComputeValue(e.mousePosition.x, trackRect, min, max, step, rtl);
                draftValue.Current = computed;
                if (live && !Mathf.Approximately(computed, value)) {
                    onChange?.Invoke(computed);
                    lastFireFrame.Current = Time.frameCount;
                }
                e.Use();
            }
            else if ((e.type == EventType.MouseUp || e.rawType == EventType.MouseUp) && dragging.Current) {
                dragging.Current = false;
                float final = draftValue.Current;
                if (!Mathf.Approximately(final, value)) {
                    onChange?.Invoke(final);
                    lastFireFrame.Current = Time.frameCount;
                }
                if (e.type == EventType.MouseUp) {
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseDrag && dragging.Current) {
                if (live && !Mathf.Approximately(draftValue.Current, value)) {
                    int frame = Time.frameCount;
                    int throttle = Mathf.Max(0, liveThrottleFrames);
                    if (frame - lastFireFrame.Current >= throttle) {
                        onChange?.Invoke(draftValue.Current);
                        lastFireFrame.Current = frame;
                    }
                }
                e.Use();
            }
        };

        return node;
    }

    private static float ComputeValue(
        float mouseX,
        Rect trackRect,
        float min,
        float max,
        float step,
        bool rtl
    ) {
        if (trackRect.width <= 0f) {
            return min;
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

        return newValue;
    }

    [DocVariant("CL_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<float> s = UseState(0.4f);
        return new DocSample(() => Create(s.Value, v => s.Set(v), disabled: forced));
    }

    [DocVariant("CL_Playground_Label_Accented")]
    public static DocSample DocsAccented() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<float> s = UseState(0.4f);
        return new DocSample(() => Create(
            s.Value,
            v => s.Set(v),
            0f,
            1f,
            0.25f,
            new[] { 0f, 0.25f, 0.5f, 0.75f, 1f },
            disabled: forced
        ));
    }

    [DocState("CL_Playground_Label_Default")]
    public static DocSample DocsDefaultState() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<float> s = UseState(0.4f);
        return new DocSample(() => Create(s.Value, v => s.Set(v), disabled: forced));
    }

    [DocState("CL_Playground_Label_Hover")]
    public static DocSample DocsHover() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<float> s = UseState(0.7f);
        return new DocSample(() => Create(s.Value, v => s.Set(v), disabled: forced));
    }

    [DocState("CL_Playground_Label_Disabled")]
    public static DocSample DocsDisabled() {
        StateHandle<float> s = UseState(0.4f);
        return new DocSample(() => Create(s.Value, v => s.Set(v), disabled: true));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        StateHandle<float> s = UseState(0.5f);
        return new DocSample(() => Create(s.Value, v => s.Set(v)));
    }
}