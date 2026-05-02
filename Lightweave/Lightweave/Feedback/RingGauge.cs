using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using static Cosmere.Lightweave.Doc.DocChips;

namespace Cosmere.Lightweave.Feedback;

[Doc(
    Id = "ringgauge",
    Summary = "Circular determinate gauge showing fractional progress.",
    WhenToUse = "Compact dial-style readout for a single 0-1 value.",
    SourcePath = "Lightweave/Lightweave/Feedback/RingGauge.cs"
)]
public static class RingGauge {
    public static LightweaveNode Create(
        [DocParam("Fill fraction in [0, 1].")]
        float value,
        [DocParam("Optional centered label text.")]
        string? centerLabel = null,
        [DocParam("Ring stroke thickness. Defaults to 0.25rem.")]
        Rem thickness = default,
        [DocParam("Color of the filled arc. Defaults to SurfaceAccent.")]
        ThemeSlot? fillColor = null,
        [DocParam("Color of the unfilled track. Defaults to BorderDefault.")]
        ThemeSlot? trackColor = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Rem resolvedThickness = thickness.Equals(default) ? new Rem(0.25f) : thickness;
        ThemeSlot resolvedFill = fillColor ?? ThemeSlot.SurfaceAccent;
        ThemeSlot resolvedTrack = trackColor ?? ThemeSlot.BorderDefault;

        LightweaveNode node = NodeBuilder.New("RingGauge", line, file);
        node.PreferredHeight = new Rem(4f).ToPixels();

        node.Paint = (rect, paintChildren) => {
            Theme.Theme theme = RenderContext.Current.Theme;

            float clamped = Mathf.Clamp01(value);
            float cx = rect.x + rect.width * 0.5f;
            float cy = rect.y + rect.height * 0.5f;
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f - resolvedThickness.ToPixels() * 0.5f;
            float lineWidth = resolvedThickness.ToPixels();

            Color trackCol = theme.GetColor(resolvedTrack);
            Color fillCol = theme.GetColor(resolvedFill);

            Color saved = GUI.color;

            // Draw track (full circle, 60 segments)
            int totalSegments = 60;
            float segStep = 360f / totalSegments;

            GUI.color = trackCol;
            for (int i = 0; i < totalSegments; i++) {
                float a0 = i * segStep;
                float a1 = (i + 1) * segStep;
                float rad0 = a0 * Mathf.Deg2Rad;
                float rad1 = a1 * Mathf.Deg2Rad;
                Vector2 p0 = new Vector2(cx + Mathf.Sin(rad0) * radius, cy - Mathf.Cos(rad0) * radius);
                Vector2 p1 = new Vector2(cx + Mathf.Sin(rad1) * radius, cy - Mathf.Cos(rad1) * radius);
                Widgets.DrawLine(p0, p1, trackCol, lineWidth);
            }

            // Draw fill arc (clockwise from 12 o'clock)
            if (clamped > 0f) {
                int fillSegments = Mathf.Max(1, Mathf.RoundToInt(clamped * totalSegments));
                GUI.color = fillCol;
                for (int i = 0; i < fillSegments; i++) {
                    float a0 = i * segStep;
                    float a1 = Mathf.Min((i + 1) * segStep, clamped * 360f);
                    float rad0 = a0 * Mathf.Deg2Rad;
                    float rad1 = a1 * Mathf.Deg2Rad;
                    Vector2 p0 = new Vector2(cx + Mathf.Sin(rad0) * radius, cy - Mathf.Cos(rad0) * radius);
                    Vector2 p1 = new Vector2(cx + Mathf.Sin(rad1) * radius, cy - Mathf.Cos(rad1) * radius);
                    Widgets.DrawLine(p0, p1, fillCol, lineWidth);
                }
            }

            GUI.color = saved;

            // Draw centered label
            if (!string.IsNullOrEmpty(centerLabel)) {
                Font font = theme.GetFont(FontRole.Body);
                int pixelSize = Mathf.RoundToInt(new Rem(0.75f).ToFontPx());
                GUIStyle style = GuiStyleCache.Get(font, pixelSize);
                style.alignment = TextAnchor.MiddleCenter;

                Color labelColor = theme.GetColor(ThemeSlot.TextPrimary);
                GUI.color = labelColor;
                GUI.Label(RectSnap.Snap(rect), centerLabel, style);
                GUI.color = saved;
            }

            paintChildren();
        };

        return node;
    }

    [DocVariant("CC_Playground_Label_Small")]
    public static DocSample DocsLow() {
        return new DocSample(CenterFixed(RingGauge.Create(0.25f), 72f, 72f));
    }

    [DocVariant("CC_Playground_Label_Medium")]
    public static DocSample DocsMid() {
        return new DocSample(CenterFixed(RingGauge.Create(0.6f), 72f, 72f));
    }

    [DocVariant("CC_Playground_Label_Large")]
    public static DocSample DocsHigh() {
        return new DocSample(CenterFixed(RingGauge.Create(0.95f), 72f, 72f));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(CenterFixed(RingGauge.Create(0.6f), 72f, 72f));
    }
}