using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using static Cosmere.Lightweave.Doc.DocChips;

namespace Cosmere.Lightweave.Layout;

[Doc(
    Id = "column",
    Summary = "Vertical flow that hugs intrinsic child heights when possible.",
    WhenToUse = "Stack content vertically with consistent spacing and natural heights.",
    SourcePath = "Lightweave/Lightweave/Layout/Column.cs",
    PreferredVariantHeight = 160f
)]
public static class Column {
    public static LightweaveNode Create(
        [DocParam("Gap between rows.")]
        Rem gap = default,
        [DocParam("Cross-axis alignment of children.")]
        FlexAlign align = FlexAlign.Stretch,
        [DocParam("Main-axis distribution.")]
        FlexJustify justify = FlexJustify.Start,
        [DocParam("Builder callback to populate children.")]
        Action<List<LightweaveNode>>? children = null,
        [DocParam("Inline style override.", TypeOverride = "Style?", DefaultOverride = "null")]
        Style? style = null,
        [DocParam("Additional class names merged after the base 'column' class.", TypeOverride = "string[]?", DefaultOverride = "null")]
        string[]? classes = null,
        [DocParam("Stable id for state-style lookup.", TypeOverride = "string?", DefaultOverride = "null")]
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        List<LightweaveNode> kids = new List<LightweaveNode>();
        children?.Invoke(kids);
        LightweaveNode node = NodeBuilder.New("Column", line, file);
        node.ApplyStyling("column", style, classes, id);
        node.Children.AddRange(kids);

        float ResolveGapPx() {
            return gap.ToPixels();
        }

        bool AllKidsKnown() {
            for (int i = 0; i < kids.Count; i++) {
                if (kids[i].Measure == null && !kids[i].PreferredHeight.HasValue) {
                    return false;
                }
            }

            return true;
        }

        float ChildHeight(LightweaveNode child, float width) {
            return child.Measure?.Invoke(width) ?? child.PreferredHeight ?? 0f;
        }

        if (AllKidsKnown()) {
            node.Measure = width => {
                int count = kids.Count;
                if (count == 0) {
                    return 0f;
                }

                float total = 0f;
                for (int i = 0; i < count; i++) {
                    total += ChildHeight(kids[i], width);
                }

                total += ResolveGapPx() * (count - 1);
                return total;
            };
        }

        node.Paint = (rect, paintChildren) => {
            float gapPx = ResolveGapPx();
            int count = kids.Count;
            if (count == 0) {
                return;
            }

            bool useIntrinsic = AllKidsKnown();
            float[] heights = new float[count];
            float totalH = 0f;
            if (useIntrinsic) {
                for (int i = 0; i < count; i++) {
                    heights[i] = ChildHeight(kids[i], rect.width);
                    totalH += heights[i];
                }
            }
            else {
                float eachH = (rect.height - gapPx * Mathf.Max(0, count - 1)) / count;
                for (int i = 0; i < count; i++) {
                    heights[i] = eachH;
                    totalH += eachH;
                }
            }

            float used = totalH + gapPx * Mathf.Max(0, count - 1);
            float startY = rect.y;
            float spacing = gapPx;
            switch (justify) {
                case FlexJustify.End:
                    startY = rect.y + rect.height - used;
                    break;
                case FlexJustify.Center:
                    startY = rect.y + (rect.height - used) * 0.5f;
                    break;
                case FlexJustify.SpaceBetween:
                    spacing = count > 1 ? (rect.height - totalH) / (count - 1) : 0f;
                    break;
                case FlexJustify.SpaceAround:
                    spacing = (rect.height - totalH) / count;
                    startY = rect.y + spacing * 0.5f;
                    break;
                case FlexJustify.SpaceEvenly:
                    spacing = (rect.height - totalH) / (count + 1);
                    startY = rect.y + spacing;
                    break;
            }

            float y = startY;
            for (int i = 0; i < count; i++) {
                LightweaveNode child = kids[i];
                float cw;
                float cx;
                if (align == FlexAlign.Stretch) {
                    cw = rect.width;
                    cx = rect.x;
                }
                else {
                    float intrinsicW = child.MeasureWidth?.Invoke() ?? rect.width;
                    cw = Mathf.Min(intrinsicW, rect.width);
                    switch (align) {
                        case FlexAlign.Center:
                            cx = rect.x + (rect.width - cw) * 0.5f;
                            break;
                        case FlexAlign.End:
                            cx = rect.x + rect.width - cw;
                            break;
                        default:
                            cx = rect.x;
                            break;
                    }
                }

                child.MeasuredRect = new Rect(cx, y, cw, heights[i]);
                y += heights[i] + spacing;
            }

            paintChildren();
        };
        return node;
    }

    [DocVariant("CL_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(() => 
            Column.Create(
                SpacingScale.Xxs,
                children: k => {
                    k.Add(SampleChip("1"));
                    k.Add(SampleChip("2"));
                    k.Add(SampleChip("3"));
                }
            )
        );
    }


    [DocVariant("CL_Playground_Label_Gap")]
    public static DocSample DocsGap() {
        return new DocSample(() => 
            Column.Create(
                SpacingScale.Md,
                children: k => {
                    k.Add(SampleChip("first"));
                    k.Add(SampleChip("second"));
                    k.Add(SampleChip("third"));
                }
            )
        );
    }

    [DocVariant("CL_Playground_Label_Align")]
    public static DocSample DocsAlign() {
        return new DocSample(() => 
            Column.Create(
                SpacingScale.Xs,
                align: FlexAlign.Center,
                children: k => {
                    k.Add(SampleChip("centered"));
                    k.Add(SampleChip("on"));
                    k.Add(SampleChip("the X axis"));
                }
            )
        );
    }

    [DocVariant("CL_Playground_Label_Justify")]
    public static DocSample DocsJustify() {
        return new DocSample(() => 
            Column.Create(
                SpacingScale.Xs,
                justify: FlexJustify.SpaceBetween,
                children: k => {
                    k.Add(SampleChip("top"));
                    k.Add(SampleChip("middle"));
                    k.Add(SampleChip("bottom"));
                }
            )
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => 
            Column.Create(
                SpacingScale.Xxs,
                children: k => {
                    k.Add(SampleChip("first"));
                    k.Add(SampleChip("second"));
                    k.Add(SampleChip("third"));
                }
            )
        );
    }
}
