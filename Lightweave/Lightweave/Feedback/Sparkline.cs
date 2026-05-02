using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Feedback;

[Doc(
    Id = "sparkline",
    Summary = "Compact inline trend line for a sequence of values.",
    WhenToUse = "Dense readouts where shape matters more than exact numbers.",
    SourcePath = "Lightweave/Lightweave/Feedback/Sparkline.cs"
)]
public static class Sparkline {
    public static LightweaveNode Create(
        [DocParam("Ordered samples to plot.")]
        IReadOnlyList<float> samples,
        [DocParam("Color of the line. Defaults to SurfaceAccent.")]
        ThemeSlot? lineColor = null,
        [DocParam("Optional fill color rendered under the line at low alpha.")]
        ThemeSlot? fillColor = null,
        [DocParam("Stroke thickness of the line.")]
        Rem lineThickness = default,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Rem resolvedThickness = lineThickness.Equals(default) ? new Rem(1f / 16f) : lineThickness;
        ThemeSlot resolvedLine = lineColor ?? ThemeSlot.SurfaceAccent;

        LightweaveNode node = NodeBuilder.New("Sparkline", line, file);
        node.PreferredHeight = new Rem(2f).ToPixels();

        node.Paint = (rect, paintChildren) => {
            if (samples == null || samples.Count == 0) {
                paintChildren();
                return;
            }

            Theme.Theme theme = RenderContext.Current.Theme;
            Color lineCol = theme.GetColor(resolvedLine);
            float lw = resolvedThickness.ToPixels();

            // Find min/max
            float min = samples[0];
            float max = samples[0];
            for (int i = 1; i < samples.Count; i++) {
                if (samples[i] < min) min = samples[i];
                if (samples[i] > max) max = samples[i];
            }

            float range = max - min;
            bool flat = range < 0.0001f;

            Color saved = GUI.color;

            if (samples.Count == 1) {
                // Single sample - draw a dot
                float dotX = rect.x + rect.width * 0.5f;
                float dotY = rect.y + rect.height * 0.5f;
                float dotSize = lw * 3f;
                Rect dotRect = new Rect(dotX - dotSize * 0.5f, dotY - dotSize * 0.5f, dotSize, dotSize);
                GUI.color = lineCol;
                GUI.DrawTexture(dotRect, Texture2D.whiteTexture);
                GUI.color = saved;
                paintChildren();
                return;
            }

            // Map sample index to x, value to y
            float xStep = rect.width / (samples.Count - 1);

            // Fill under the line (approximate polygon as vertical bars)
            if (fillColor.HasValue) {
                Color fillCol = theme.GetColor(fillColor.Value);
                Color fillDraw = new Color(fillCol.r, fillCol.g, fillCol.b, fillCol.a * 0.25f);

                for (int i = 0; i < samples.Count - 1; i++) {
                    float x0 = rect.x + i * xStep;
                    float x1 = rect.x + (i + 1) * xStep;
                    float norm0 = flat ? 0.5f : (samples[i] - min) / range;
                    float norm1 = flat ? 0.5f : (samples[i + 1] - min) / range;

                    float y0 = rect.yMax - norm0 * rect.height;
                    float y1 = rect.yMax - norm1 * rect.height;

                    // Draw vertical bars from bottom up to the interpolated line
                    float colWidth = x1 - x0;
                    int barCount = Mathf.Max(1, Mathf.RoundToInt(colWidth));
                    for (int b = 0; b < barCount; b++) {
                        float t = (float)b / barCount;
                        float bx = x0 + t * colWidth;
                        float lineY = Mathf.Lerp(y0, y1, t);
                        float barHeight = rect.yMax - lineY;
                        if (barHeight > 0f) {
                            Rect bar = new Rect(bx, lineY, 1f, barHeight);
                            Widgets.DrawBoxSolid(bar, fillDraw);
                        }
                    }
                }
            }

            // Draw the line segments
            GUI.color = lineCol;
            for (int i = 0; i < samples.Count - 1; i++) {
                float x0 = rect.x + i * xStep;
                float x1 = rect.x + (i + 1) * xStep;
                float norm0 = flat ? 0.5f : (samples[i] - min) / range;
                float norm1 = flat ? 0.5f : (samples[i + 1] - min) / range;

                float y0 = rect.yMax - norm0 * rect.height;
                float y1 = rect.yMax - norm1 * rect.height;

                Vector2 p0 = new Vector2(x0, y0);
                Vector2 p1 = new Vector2(x1, y1);
                Widgets.DrawLine(p0, p1, lineCol, lw);
            }

            GUI.color = saved;
            paintChildren();
        };

        return node;
    }

    [DocVariant("CC_Playground_Label_Accent")]
    public static DocSample DocsRising() {
        float[] rising = new[] { 1f, 2f, 3f, 5f, 8f, 13f };
        return new DocSample(Sparkline.Create(rising));
    }

    [DocVariant("CC_Playground_Label_Default")]
    public static DocSample DocsWavy() {
        float[] wavy = new[] { 3f, 5f, 2f, 7f, 4f, 6f, 2f };
        return new DocSample(Sparkline.Create(wavy));
    }

    [DocVariant("CC_Playground_Label_Muted")]
    public static DocSample DocsFlat() {
        float[] flat = new[] { 4f, 4f, 4f, 4f, 4f };
        return new DocSample(Sparkline.Create(flat, ThemeSlot.TextMuted));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        float[] rising = new[] { 1f, 2f, 3f, 5f, 8f, 13f };
        return new DocSample(Sparkline.Create(rising));
    }
}