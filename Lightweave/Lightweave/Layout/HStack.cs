using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using static Cosmere.Lightweave.Doc.DocChips;

namespace Cosmere.Lightweave.Layout;

public sealed class HStackBuilder {
    internal readonly List<(LightweaveNode node, float width, bool flex, bool hug)> Items =
        new List<(LightweaveNode, float, bool, bool)>();

    public void Add(LightweaveNode node, float width) {
        Items.Add((node, width, false, false));
    }

    public void AddFlex(LightweaveNode node) {
        Items.Add((node, 0f, true, false));
    }

    public void AddHug(LightweaveNode node) {
        Items.Add((node, 0f, false, true));
    }
}

[Doc(
    Id = "hstack",
    Summary = "Horizontal layout with explicit per-item width (fixed or flex).",
    WhenToUse = "Lay out a row where some items are fixed-width and others absorb leftover space.",
    SourcePath = "Lightweave/Lightweave/Layout/HStack.cs",
    ShowRtl = true
)]
public static class HStack {
    public static LightweaveNode Create(
        [DocParam("Gap between columns.", TypeOverride = "Responsive<Rem>", DefaultOverride = "0")]
        Responsive<Rem> gap = default,
        [DocParam("Builder callback to populate items via Add(width) / AddFlex().")]
        Action<HStackBuilder>? children = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        HStackBuilder builder = new HStackBuilder();
        children?.Invoke(builder);

        LightweaveNode node = NodeBuilder.New("HStack", line, file);
        int count = builder.Items.Count;
        for (int i = 0; i < count; i++) {
            node.Children.Add(builder.Items[i].node);
        }

        float ResolveGapPx() {
            return gap.Resolve(RenderContext.Current.Breakpoint).ToPixels();
        }

        float? maxChildHeight = null;
        for (int i = 0; i < count; i++) {
            float? ph = builder.Items[i].node.PreferredHeight;
            if (ph.HasValue && (!maxChildHeight.HasValue || ph.Value > maxChildHeight.Value)) {
                maxChildHeight = ph.Value;
            }
        }

        node.PreferredHeight = maxChildHeight;

        float[] AllocateWidths(float availableWidth, float gapPx, bool[] inFlow) {
            float[] widths = new float[count];
            float fixedTotal = 0f;
            int flexCount = 0;
            int flowCount = 0;
            for (int i = 0; i < count; i++) {
                if (!inFlow[i]) {
                    widths[i] = 0f;
                    continue;
                }
                flowCount++;
                var item = builder.Items[i];
                if (item.flex) {
                    flexCount++;
                }
                else if (item.hug) {
                    float w = item.node.MeasureWidth?.Invoke() ?? 0f;
                    widths[i] = w;
                    fixedTotal += w;
                }
                else {
                    widths[i] = item.width;
                    fixedTotal += item.width;
                }
            }

            float totalGap = gapPx * Math.Max(0, flowCount - 1);
            float remainingForFlex = Mathf.Max(0f, availableWidth - fixedTotal - totalGap);
            float flexEach = flexCount > 0 ? remainingForFlex / flexCount : 0f;
            for (int i = 0; i < count; i++) {
                if (inFlow[i] && builder.Items[i].flex) {
                    widths[i] = flexEach;
                }
            }

            return widths;
        }

        float MeasureChildHeight(LightweaveNode child, float width) {
            if (child.Measure != null) {
                return child.Measure(width);
            }

            return child.PreferredHeight ?? 0f;
        }

        node.MeasureWidth = () => {
            if (count == 0) {
                return 0f;
            }
            float gapPx = ResolveGapPx();
            float total = 0f;
            int counted = 0;
            for (int i = 0; i < count; i++) {
                if (!builder.Items[i].node.IsInFlow()) {
                    continue;
                }
                var item = builder.Items[i];
                if (item.flex) {
                    continue;
                }
                if (item.hug) {
                    total += item.node.MeasureWidth?.Invoke() ?? 0f;
                }
                else {
                    total += item.width;
                }
                counted++;
            }
            float totalGap = gapPx * Math.Max(0, counted - 1);
            return total + totalGap;
        };

        node.Measure = availableWidth => {
            if (count == 0) {
                return 0f;
            }

            bool[] inFlow = new bool[count];
            for (int i = 0; i < count; i++) {
                inFlow[i] = builder.Items[i].node.IsInFlow();
            }

            float[] widths = AllocateWidths(availableWidth, ResolveGapPx(), inFlow);
            float max = 0f;
            for (int i = 0; i < count; i++) {
                if (!inFlow[i]) {
                    continue;
                }
                float h = MeasureChildHeight(builder.Items[i].node, widths[i]);
                if (h > max) {
                    max = h;
                }
            }

            return max;
        };

        node.Paint = (rect, paintChildren) => {
            if (count == 0) {
                return;
            }

            Direction dir = RenderContext.Current.Direction;
            bool reverse = dir == Direction.Rtl;

            float gapPx = ResolveGapPx();
            bool[] inFlow = new bool[count];
            for (int i = 0; i < count; i++) {
                inFlow[i] = builder.Items[i].node.IsInFlow();
            }
            float[] widths = AllocateWidths(rect.width, gapPx, inFlow);

            float x = rect.x;
            for (int i = 0; i < count; i++) {
                int idx = reverse ? count - 1 - i : i;
                if (!inFlow[idx]) {
                    continue;
                }
                LightweaveNode child = builder.Items[idx].node;
                float w = widths[idx];
                child.MeasuredRect = new Rect(x, rect.y, w, rect.height);
                x += w + gapPx;
            }

            paintChildren();
        };

        return node;
    }

    [DocVariant("CL_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(() => 
            HStack.Create(
                SpacingScale.Xs,
                r => {
                    r.Add(SampleChip("48"), 48f);
                    r.AddFlex(AccentChip("flex"));
                    r.Add(SampleChip("32"), 32f);
                }
            )
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => 
            HStack.Create(
                SpacingScale.Xs,
                r => {
                    r.Add(SampleChip("left"), 48f);
                    r.AddFlex(AccentChip("center"));
                    r.Add(SampleChip("right"), 48f);
                }
            )
        );
    }
}
