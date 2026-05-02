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

namespace Cosmere.Lightweave.Input;

[Doc(
    Id = "switch",
    Summary = "Animated on/off toggle with adjacent label.",
    WhenToUse = "Toggle a setting that takes effect immediately.",
    SourcePath = "Lightweave/Lightweave/Input/Switch.cs"
)]
public static class Switch {
    private const float AnimationDurationSec = 0.12f;

    public static LightweaveNode Create(
        [DocParam("Text rendered next to the track.")]
        string label,
        [DocParam("Current on/off state.")]
        bool value,
        [DocParam("Invoked with the new value when toggled.")]
        Action<bool> onChange,
        [DocParam("Disables interaction and applies disabled styling.")]
        bool disabled = false,
        [DocParam("Optional key disambiguating multiple instances declared on the same line.")]
        object? instanceKey = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        string keySuffix = instanceKey == null ? string.Empty : "#" + instanceKey;
        string progressKey = file + "#sw_progress" + keySuffix;
        string lastTimeKey = file + "#sw_lastTime" + keySuffix;

        LightweaveNode node = NodeBuilder.New($"Switch:{label}", line, file);
        node.PreferredHeight = new Rem(1.75f).ToPixels();

        node.Paint = (rect, paintChildren) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            float trackWidth = new Rem(2.5f).ToPixels();
            float trackHeight = new Rem(1.25f).ToPixels();
            float thumbSize = new Rem(1f).ToPixels();
            float thumbInset = new Rem(0.125f).ToPixels();
            float rowHeight = new Rem(1.75f).ToPixels();
            float gapPx = new Rem(0.5f).ToPixels();

            float rowY = rect.y;
            float trackY = rowY + (rowHeight - trackHeight) / 2f;

            float trackX = rtl ? rect.xMax - trackWidth : rect.x;
            Rect trackRect = new Rect(trackX, trackY, trackWidth, trackHeight);

            float labelX = rtl ? rect.x : trackX + trackWidth + gapPx;
            float labelWidth = rtl
                ? trackX - gapPx - rect.x
                : rect.xMax - labelX;
            Rect labelRect = new Rect(labelX, rowY, Mathf.Max(0f, labelWidth), rowHeight);

            Rect hitRect = new Rect(rect.x, rowY, rect.width, rowHeight);
            LightweaveHitTracker.Track(hitRect);

            Hooks.Hooks.RefHandle<float> progress = Hooks.Hooks.UseRef(value ? 1f : 0f, line, progressKey);
            Hooks.Hooks.RefHandle<float> lastTime = Hooks.Hooks.UseRef(
                Time.realtimeSinceStartup,
                line,
                lastTimeKey
            );

            float now = Time.realtimeSinceStartup;
            float dt = Mathf.Max(0f, now - lastTime.Current);
            lastTime.Current = now;

            float target = value ? 1f : 0f;
            if (!Mathf.Approximately(progress.Current, target)) {
                float delta = dt / AnimationDurationSec;
                if (target > progress.Current) {
                    progress.Current = Mathf.Min(target, progress.Current + delta);
                } else {
                    progress.Current = Mathf.Max(target, progress.Current - delta);
                }
            }

            float animFraction = Mathf.Clamp01(progress.Current);

            bool mouseOver = Mouse.IsOver(hitRect);
            bool hovered = !disabled && mouseOver;
            if (!disabled) {
                MouseoverSounds.DoRegion(hitRect);
            } else if (mouseOver) {
                CursorOverrides.MarkDisabledHover();
            }

            InteractionState trackState = new InteractionState(hovered, false, false, disabled);
            ThemeSlot borderSlot = InputSurface.ResolveToggleBorderSlot(trackState, value);

            ThemeSlot trackFill = disabled
                ? ThemeSlot.SurfaceDisabled
                : value
                    ? ThemeSlot.SurfaceAccent
                    : ThemeSlot.SurfaceInput;
            BackgroundSpec trackBg = new BackgroundSpec.Solid(trackFill);
            BorderSpec trackBorder = BorderSpec.All(new Rem(1f / 16f), borderSlot);
            RadiusSpec trackRadius = RadiusSpec.All(new Rem(0.625f));
            PaintBox.Draw(trackRect, trackBg, trackBorder, trackRadius);

            float leftX = trackRect.x + thumbInset;
            float rightX = trackRect.xMax - thumbInset - thumbSize;
            float thumbX = rtl
                ? Mathf.Lerp(rightX, leftX, animFraction)
                : Mathf.Lerp(leftX, rightX, animFraction);
            float thumbY = trackRect.y + (trackHeight - thumbSize) / 2f;
            Rect thumbRect = new Rect(thumbX, thumbY, thumbSize, thumbSize);

            ThemeSlot thumbSlot = disabled
                ? ThemeSlot.BorderOff
                : value
                    ? ThemeSlot.TextOnAccent
                    : ThemeSlot.BorderOff;
            BackgroundSpec thumbBg = new BackgroundSpec.Solid(thumbSlot);
            RadiusSpec thumbRadius = RadiusSpec.All(new Rem(0.5f));
            PaintBox.Draw(thumbRect, thumbBg, null, thumbRadius);

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

    [DocVariant("CC_Playground_Label_On")]
    public static DocSample DocsOn() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(Create(
            (string)"CC_Playground_Controls_Switch_Label".Translate(),
            true,
            _ => { },
            forced
        ));
    }

    [DocVariant("CC_Playground_Label_Off")]
    public static DocSample DocsOff() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(Create("Off", false, _ => { }, forced));
    }

    [DocState("CC_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(Create("Default", true, _ => { }, forced));
    }

    [DocState("CC_Playground_Label_Hover")]
    public static DocSample DocsHover() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(Create("Hover", false, _ => { }, forced));
    }

    [DocState("CC_Playground_Label_Disabled")]
    public static DocSample DocsDisabled() {
        return new DocSample(Create("Disabled", true, _ => { }, true));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(Create("Enabled", true, _ => { }));
    }
}