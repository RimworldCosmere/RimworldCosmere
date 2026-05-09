using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Hooks;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using static Cosmere.Lightweave.Hooks.Hooks;

namespace Cosmere.Lightweave.Feedback;

[Doc(
    Id = "progressbar",
    Summary = "Determinate horizontal bar showing fractional progress.",
    WhenToUse = "Display known-duration progress with an optional label.",
    SourcePath = "Lightweave/Lightweave/Feedback/ProgressBar.cs",
    ShowRtl = true
)]
public static class ProgressBar {
    public static LightweaveNode Create(
        [DocParam("Current value within [min, max].")]
        float value,
        [DocParam("Minimum value of the range.")]
        float min = 0f,
        [DocParam("Maximum value of the range.")]
        float max = 1f,
        [DocParam("Optional centered label text.")]
        string? label = null,
        [DocParam("Color variant for the fill.")]
        BadgeVariant variant = BadgeVariant.Accent,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New($"ProgressBar:{variant}", line, file);
        node.PreferredHeight = new Rem(1f).ToPixels();

        node.Paint = (rect, paintChildren) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            RadiusSpec radius = RadiusSpec.All(RadiusScale.Lg);

            BackgroundSpec trackBg = BackgroundSpec.Of(ThemeSlot.SurfaceInput);
            BorderSpec trackBorder = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault);
            PaintBox.Draw(rect, trackBg, trackBorder, radius);

            float range = max - min;
            float fraction = range > 0f ? Mathf.Clamp01((value - min) / range) : 0f;
            float fillWidth = rect.width * fraction;

            Rect fillRect = default;
            bool hasFill = fillWidth > 0f;
            if (hasFill) {
                float fillX = rtl ? rect.xMax - fillWidth : rect.x;
                fillRect = new Rect(fillX, rect.y, fillWidth, rect.height);
                BackgroundSpec fillBg = BackgroundSpec.Of(BadgeVariants.Background(variant));
                PaintBox.Draw(fillRect, fillBg, null, radius);
            }

            if (!string.IsNullOrEmpty(label)) {
                Font font = theme.GetFont(FontRole.BodyBold);
                int pixelSize = Mathf.RoundToInt(new Rem(0.75f).ToFontPx());
                GUIStyle style = GuiStyleCache.GetOrCreate(font, pixelSize, FontStyle.Bold);
                style.alignment = TextAnchor.MiddleCenter;

                float midX = rect.x + rect.width / 2f;
                bool midOverFill = hasFill && midX >= fillRect.x && midX <= fillRect.xMax;
                ThemeSlot labelSlot = midOverFill
                    ? BadgeVariants.Foreground(variant)
                    : ThemeSlot.TextPrimary;

                Color savedColor = GUI.color;
                GUI.color = theme.GetColor(labelSlot);
                GUI.Label(RectSnap.Snap(rect), label, style);
                GUI.color = savedColor;
            }

            paintChildren();
        };

        return node;
    }

    [DocVariant("CL_Playground_Label_Accent")]
    public static DocSample DocsAccent() {
        return new DocSample(() => ProgressBar.Create(0.65f, 0f, 1f, "65%"));
    }

    [DocVariant("CL_Playground_Label_Default")]
    public static DocSample DocsSuccess() {
        return new DocSample(() => ProgressBar.Create(0.35f, 0f, 1f, "35%", BadgeVariant.Success));
    }

    [DocVariant("CL_Playground_Label_Danger")]
    public static DocSample DocsDanger() {
        return new DocSample(() => ProgressBar.Create(0.9f, 0f, 1f, "90%", BadgeVariant.Danger));
    }

    [DocVariant("CL_Playground_Feedback_ProgressBar_Animated", Order = 4)]
    public static DocSample DocsAnimated() {
        return new DocSample(() => {
            RefHandle<float> startRef = UseRef(-1f);
            if (startRef.Current < 0f) {
                startRef.Current = Time.unscaledTime;
            }

            const float rise = 5f;
            const float hold = 2f;
            const float fall = 0.3f;
            const float cycle = rise + hold + fall;

            float t = (Time.unscaledTime - startRef.Current) % cycle;
            float value;
            if (t < rise) {
                float u = 1f - (t / rise);
                value = 1f - (u * u * u);
            }
            else if (t < rise + hold) {
                value = 1f;
            }
            else {
                float k = (t - rise - hold) / fall;
                value = Mathf.Lerp(1f, 0f, k);
            }

            AnimationClock.RegisterActive(RenderContext.Current.RootId);

            int pct = Mathf.RoundToInt(value * 100f);
            return ProgressBar.Create(value, 0f, 1f, $"{pct}%");
        });
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => ProgressBar.Create(0.65f, 0f, 1f, "65%"));
    }
}