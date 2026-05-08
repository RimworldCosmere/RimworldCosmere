using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Feedback;

[Doc(
    Id = "chart",
    Summary = "Inline data visualization with optional axes, hover, and tooltips.",
    WhenToUse = "Trend lines, sparklines, or compact time-series readouts. Add axes for richer dashboards or stay compact for dense UIs.",
    SourcePath = "Lightweave/Lightweave/Feedback/Chart.cs",
    PreferredVariantHeight = 200f
)]
public static class Chart {
    public static LightweaveNode Create(
        [DocParam("Ordered samples to plot.")]
        IReadOnlyList<float> samples,
        [DocParam("Color of the line. Defaults to SurfaceAccent.")]
        ThemeSlot? lineColor = null,
        [DocParam("Optional fill color rendered under the line at low alpha.")]
        ThemeSlot? fillColor = null,
        [DocParam("Stroke thickness of the line.")]
        Rem lineThickness = default,
        [DocParam("Render X-axis ticks along the bottom gutter.")]
        bool showXAxis = false,
        [DocParam("Render Y-axis ticks and grid lines on the left gutter.")]
        bool showYAxis = false,
        [DocParam("Caption rendered below the X-axis.")]
        string? xAxisLabel = null,
        [DocParam("Caption rendered above the Y-axis.")]
        string? yAxisLabel = null,
        [DocParam("Format an X-axis tick value (sample index) as a string.")]
        Func<int, string>? xTickFormatter = null,
        [DocParam("Format a Y-axis tick value as a string.")]
        Func<float, string>? yTickFormatter = null,
        [DocParam("Number of X-axis ticks. Default 5.")]
        int tickCountX = 5,
        [DocParam("Number of Y-axis ticks. Default 5.")]
        int tickCountY = 5,
        [DocParam("Show a tooltip with the nearest sample on hover.")]
        bool showTooltip = false,
        [DocParam("Format the tooltip text given the sample index and value.")]
        Func<int, float, string>? tooltipFormatter = null,
        [DocParam("Anchor side for the tooltip relative to the hovered point. Default Top.")]
        TooltipSide tooltipSide = TooltipSide.Top,
        [DocParam("Pixel gap between the hovered point and the tooltip on the anchor axis.")]
        float tooltipSideOffset = 4f,
        [DocParam("Hover seconds before the tooltip appears. 0 = instant.")]
        float tooltipDelay = 0.05f,
        [DocParam("Highlight the nearest sample with a marker dot on hover.")]
        bool pointHighlight = false,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Rem resolvedThickness = lineThickness.Equals(default) ? new Rem(1f / 16f) : lineThickness;
        ThemeSlot resolvedLine = lineColor ?? ThemeSlot.SurfaceAccent;

        LightweaveNode node = NodeBuilder.New("Chart", line, file);
        node.PreferredHeight = new Rem(2f).ToPixels();

        ChartHoverState[] stateRef = { default };

        node.Paint = (rect, paintChildren) => {
            if (samples == null || samples.Count == 0) {
                paintChildren();
                return;
            }

            Theme.Theme theme = RenderContext.Current.Theme;
            Color lineCol = theme.GetColor(resolvedLine);
            Color textCol = theme.GetColor(ThemeSlot.TextSecondary);
            Color mutedCol = theme.GetColor(ThemeSlot.TextMuted);
            float lw = resolvedThickness.ToPixels();

            float min = samples[0];
            float max = samples[0];
            for (int i = 1; i < samples.Count; i++) {
                if (samples[i] < min) {
                    min = samples[i];
                }

                if (samples[i] > max) {
                    max = samples[i];
                }
            }

            float range = max - min;
            bool flat = range < 0.0001f;

            float yAxisGutter = showYAxis ? (yAxisLabel != null ? 56f : 36f) : 0f;
            float xAxisGutter = showXAxis ? (xAxisLabel != null ? 32f : 18f) : 0f;
            float topPad = (showYAxis && yAxisLabel != null) ? 18f : (showYAxis ? 8f : 0f);

            Rect plotRect = new Rect(
                rect.x + yAxisGutter,
                rect.y + topPad,
                Mathf.Max(0f, rect.width - yAxisGutter),
                Mathf.Max(0f, rect.height - xAxisGutter - topPad)
            );
            stateRef[0] = new ChartHoverState {
                Plot = plotRect,
                Min = min,
                Max = max,
                Flat = flat,
            };

            Font font = theme.GetFont(FontRole.Body);
            int tickPx = Mathf.RoundToInt(new Rem(0.65f).ToFontPx());
            int labelPx = Mathf.RoundToInt(new Rem(0.75f).ToFontPx());
            GUIStyle tickStyle = GuiStyleCache.GetOrCreate(font, tickPx);
            GUIStyle labelStyle = GuiStyleCache.GetOrCreate(font, labelPx);

            Color saved = GUI.color;

            if (showYAxis && tickCountY >= 2) {
                tickStyle.alignment = TextAnchor.MiddleRight;
                Color gridCol = new Color(mutedCol.r, mutedCol.g, mutedCol.b, 0.18f);
                for (int i = 0; i < tickCountY; i++) {
                    float t = (float)i / (tickCountY - 1);
                    float val = Mathf.Lerp(max, min, t);
                    float yp = plotRect.y + t * plotRect.height;
                    Rect labelRect = new Rect(rect.x, yp - tickPx, yAxisGutter - 6f, tickPx * 2f);
                    GUI.color = textCol;
                    string txt = yTickFormatter != null ? yTickFormatter(val) : val.ToString("0.#");
                    GUI.Label(RectSnap.Snap(labelRect), txt, tickStyle);
                    Widgets.DrawLine(new Vector2(plotRect.x, yp), new Vector2(plotRect.xMax, yp), gridCol, 1f);
                }
            }

            if (showYAxis && yAxisLabel != null) {
                labelStyle.alignment = TextAnchor.MiddleLeft;
                GUI.color = mutedCol;
                Rect ylRect = new Rect(rect.x, rect.y, yAxisGutter + plotRect.width, topPad);
                GUI.Label(RectSnap.Snap(ylRect), yAxisLabel, labelStyle);
            }

            if (showXAxis && tickCountX >= 2) {
                tickStyle.alignment = TextAnchor.UpperCenter;
                for (int i = 0; i < tickCountX; i++) {
                    float t = (float)i / (tickCountX - 1);
                    int idx = Mathf.RoundToInt(t * (samples.Count - 1));
                    float xp = plotRect.x + t * plotRect.width;
                    Rect labelRect = new Rect(xp - 32f, plotRect.yMax + 4f, 64f, tickPx + 4f);
                    GUI.color = textCol;
                    string txt = xTickFormatter != null ? xTickFormatter(idx) : idx.ToString();
                    GUI.Label(RectSnap.Snap(labelRect), txt, tickStyle);
                }
            }

            if (showXAxis && xAxisLabel != null) {
                labelStyle.alignment = TextAnchor.MiddleCenter;
                GUI.color = mutedCol;
                Rect xlRect = new Rect(plotRect.x, rect.yMax - labelPx - 4f, plotRect.width, labelPx + 4f);
                GUI.Label(RectSnap.Snap(xlRect), xAxisLabel, labelStyle);
            }

            if (samples.Count == 1) {
                float dotX = plotRect.x + plotRect.width * 0.5f;
                float dotY = plotRect.y + plotRect.height * 0.5f;
                float dotSize = lw * 3f;
                Rect dotRect = new Rect(dotX - dotSize * 0.5f, dotY - dotSize * 0.5f, dotSize, dotSize);
                GUI.color = lineCol;
                GUI.DrawTexture(dotRect, Texture2D.whiteTexture);
                GUI.color = saved;
                paintChildren();
                return;
            }

            float xStep = plotRect.width / (samples.Count - 1);

            if (fillColor.HasValue) {
                Color fillCol = theme.GetColor(fillColor.Value);
                Color fillDraw = new Color(fillCol.r, fillCol.g, fillCol.b, fillCol.a * 0.25f);
                for (int i = 0; i < samples.Count - 1; i++) {
                    float x0 = plotRect.x + i * xStep;
                    float x1 = plotRect.x + (i + 1) * xStep;
                    float norm0 = flat ? 0.5f : (samples[i] - min) / range;
                    float norm1 = flat ? 0.5f : (samples[i + 1] - min) / range;
                    float y0 = plotRect.yMax - norm0 * plotRect.height;
                    float y1 = plotRect.yMax - norm1 * plotRect.height;
                    float colWidth = x1 - x0;
                    int barCount = Mathf.Max(1, Mathf.RoundToInt(colWidth));
                    for (int b = 0; b < barCount; b++) {
                        float t = (float)b / barCount;
                        float bx = x0 + t * colWidth;
                        float lineY = Mathf.Lerp(y0, y1, t);
                        float barHeight = plotRect.yMax - lineY;
                        if (barHeight > 0f) {
                            Rect bar = new Rect(bx, lineY, 1f, barHeight);
                            Widgets.DrawBoxSolid(bar, fillDraw);
                        }
                    }
                }
            }

            GUI.color = lineCol;
            for (int i = 0; i < samples.Count - 1; i++) {
                float x0 = plotRect.x + i * xStep;
                float x1 = plotRect.x + (i + 1) * xStep;
                float norm0 = flat ? 0.5f : (samples[i] - min) / range;
                float norm1 = flat ? 0.5f : (samples[i + 1] - min) / range;
                float y0 = plotRect.yMax - norm0 * plotRect.height;
                float y1 = plotRect.yMax - norm1 * plotRect.height;
                Vector2 p0 = new Vector2(x0, y0);
                Vector2 p1 = new Vector2(x1, y1);
                Widgets.DrawLine(p0, p1, lineCol, lw);
            }

            if (pointHighlight && plotRect.Contains(Event.current.mousePosition)) {
                float relX = Event.current.mousePosition.x - plotRect.x;
                int hoveredIdx = Mathf.Clamp(
                    Mathf.RoundToInt(relX / Mathf.Max(0.001f, plotRect.width) * (samples.Count - 1)),
                    0,
                    samples.Count - 1
                );
                float hx = plotRect.x + hoveredIdx * xStep;
                float hyNorm = flat ? 0.5f : (samples[hoveredIdx] - min) / range;
                float hy = plotRect.yMax - hyNorm * plotRect.height;

                float dotSize = lw * 5f;
                Rect dotRect = new Rect(hx - dotSize * 0.5f, hy - dotSize * 0.5f, dotSize, dotSize);
                Color halo = new Color(lineCol.r, lineCol.g, lineCol.b, 0.25f);
                float haloSize = dotSize * 2.2f;
                Rect haloRect = new Rect(hx - haloSize * 0.5f, hy - haloSize * 0.5f, haloSize, haloSize);
                GUI.color = halo;
                GUI.DrawTexture(haloRect, Texture2D.whiteTexture);
                GUI.color = lineCol;
                GUI.DrawTexture(dotRect, Texture2D.whiteTexture);
            }

            GUI.color = saved;
            paintChildren();
        };

        if (!showTooltip) {
            return node;
        }

        int sampleCount = samples.Count;
        IReadOnlyList<float> samplesCaptured = samples;
        Func<int, float, string>? formatterCaptured = tooltipFormatter;

        int HoveredIndex(out Rect plot) {
            plot = stateRef[0].Plot;
            if (sampleCount == 0 || plot.width <= 0f) {
                return -1;
            }
            Vector2 m = Event.current?.mousePosition ?? Vector2.zero;
            if (!plot.Contains(m)) {
                return -1;
            }
            float rx = m.x - plot.x;
            return Mathf.Clamp(
                Mathf.RoundToInt(rx / plot.width * (sampleCount - 1)),
                0,
                sampleCount - 1
            );
        }

        return Tooltip.Create(
            node,
            text: () => {
                int idx = HoveredIndex(out _);
                if (idx < 0) {
                    return string.Empty;
                }
                return formatterCaptured != null
                    ? formatterCaptured(idx, samplesCaptured[idx])
                    : $"{idx}: {samplesCaptured[idx]:0.##}";
            },
            side: tooltipSide,
            delayDuration: tooltipDelay,
            sideOffset: tooltipSideOffset,
            anchor: () => {
                int idx = HoveredIndex(out Rect plot);
                if (idx < 0 || sampleCount <= 1) {
                    return plot;
                }
                ChartHoverState s = stateRef[0];
                float xStep = plot.width / (sampleCount - 1);
                float hx = plot.x + idx * xStep;
                float hyNorm = s.Flat ? 0.5f : (samplesCaptured[idx] - s.Min) / Mathf.Max(0.0001f, s.Max - s.Min);
                float hy = plot.yMax - hyNorm * plot.height;
                const float pointAnchorSize = 6f;
                return new Rect(
                    hx - pointAnchorSize * 0.5f,
                    hy - pointAnchorSize * 0.5f,
                    pointAnchorSize,
                    pointAnchorSize
                );
            },
            line: line,
            file: file
        );
    }


    [DocVariant("CC_Playground_Label_Accent")]
    public static DocSample DocsRising() {
        return new DocSample(() => Chart.Create(new[] { 1f, 2f, 3f, 5f, 8f, 13f }));
    }

    [DocVariant("CC_Playground_Label_Default", Order = 1)]
    public static DocSample DocsWavy() {
        return new DocSample(() => Chart.Create(new[] { 3f, 5f, 2f, 7f, 4f, 6f, 2f }));
    }

    [DocVariant("CC_Playground_Label_Muted", Order = 2)]
    public static DocSample DocsFlat() {
        return new DocSample(() => Chart.Create(new[] { 4f, 4f, 4f, 4f, 4f }, ThemeSlot.TextMuted));
    }

    [DocVariant("CC_Playground_Chart_Axes", Order = 3)]
    public static DocSample DocsAxes() {
        return new DocSample(() => Chart.Create(
            new[] { 32f, 41f, 38f, 55f, 62f, 70f, 65f, 78f, 84f },
            fillColor: ThemeSlot.SurfaceAccent,
            showXAxis: true,
            showYAxis: true,
            xAxisLabel: (string)"CC_Playground_Chart_Axis_Day".Translate(),
            yAxisLabel: (string)"CC_Playground_Chart_Axis_Stormlight".Translate(),
            yTickFormatter: v => v.ToString("0"),
            xTickFormatter: i => $"D{i + 1}"
        ));
    }

    [DocVariant("CC_Playground_Chart_Hover", Order = 4)]
    public static DocSample DocsHover() {
        return new DocSample(() => Chart.Create(
            new[] { 12f, 18f, 15f, 22f, 28f, 24f, 30f, 26f, 35f, 32f },
            fillColor: ThemeSlot.SurfaceAccent,
            showTooltip: true,
            pointHighlight: true,
            tooltipFormatter: (i, v) => "CC_Playground_Chart_Hover_Tooltip"
                .Translate((i + 1).Named("DAY"), v.ToString("0.0").Named("VALUE"))
                .Resolve()
        ));
    }

    [DocVariant("CC_Playground_Chart_Complex", Order = 5)]
    public static DocSample DocsComplex() {
        return new DocSample(() => Chart.Create(
            new[] { 412f, 388f, 455f, 502f, 478f, 540f, 612f, 588f, 645f, 702f, 678f, 720f },
            lineColor: ThemeSlot.SurfaceAccent,
            fillColor: ThemeSlot.SurfaceAccent,
            showXAxis: true,
            showYAxis: true,
            xAxisLabel: (string)"CC_Playground_Chart_Axis_Hour".Translate(),
            yAxisLabel: (string)"CC_Playground_Chart_Axis_Reserve".Translate(),
            yTickFormatter: v => v.ToString("0"),
            xTickFormatter: i => $"{i:D2}h",
            tickCountX: 6,
            tickCountY: 5,
            showTooltip: true,
            pointHighlight: true,
            tooltipFormatter: (i, v) => "CC_Playground_Chart_Complex_Tooltip"
                .Translate(i.Named("HOUR"), v.ToString("0").Named("VALUE"))
                .Resolve()
        ));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => Chart.Create(new[] { 1f, 2f, 3f, 5f, 8f, 13f }));
    }


    private struct ChartHoverState {
        public Rect Plot;
        public float Min;
        public float Max;
        public bool Flat;
    }
}
