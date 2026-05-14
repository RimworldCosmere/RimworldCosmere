using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using static Cosmere.Lightweave.Doc.DocChips;

namespace Cosmere.Lightweave.Layout;

internal enum StackItemMode {
    Fixed,
    Flex,
    Hug,
}

public sealed class StackBuilder {
    internal readonly List<(LightweaveNode node, float height, StackItemMode mode)> Items =
        new List<(LightweaveNode, float, StackItemMode)>();

    public void Add(LightweaveNode node) {
        Items.Add((node, 0f, StackItemMode.Hug));
    }

    public void Add(LightweaveNode node, float height) {
        Items.Add((node, height, StackItemMode.Fixed));
    }

    public void AddFlex(LightweaveNode node) {
        Items.Add((node, 0f, StackItemMode.Flex));
    }
}

[Doc(
    Id = "stack",
    Summary = "Vertical layout with explicit per-item sizing (fixed, flex, hug).",
    WhenToUse = "Stack rows where some are fixed-height, some hug content, some absorb leftover space.",
    SourcePath = "Lightweave/Lightweave/Layout/Stack.cs",
    PreferredVariantHeight = 160f
)]
public static class Stack {
    public static LightweaveNode Create(
        [DocParam("Gap between stacked items.")]
        Rem gap = default,
        [DocParam("Builder callback to populate items via Add / AddFlex.")]
        Action<StackBuilder>? children = null,
        [DocParam("Inline style override.", TypeOverride = "Style?", DefaultOverride = "null")]
        Style? style = null,
        [DocParam("Additional class names merged after the base 'stack' class.", TypeOverride = "string[]?", DefaultOverride = "null")]
        string[]? classes = null,
        [DocParam("Stable id for state-style lookup.", TypeOverride = "string?", DefaultOverride = "null")]
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        StackBuilder builder = new StackBuilder();
        children?.Invoke(builder);

        LightweaveNode node = NodeBuilder.New("Stack", line, file);
        node.ApplyStyling("stack", style, classes, id);
        int count = builder.Items.Count;
        for (int i = 0; i < count; i++) {
            node.Children.Add(builder.Items[i].node);
        }

        float ResolveGapPx() {
            return gap.ToPixels();
        }

        float ResolveItemHeight(int index, float availableWidth) {
            (LightweaveNode child, float h, StackItemMode mode) = builder.Items[index];
            switch (mode) {
                case StackItemMode.Fixed:
                    return h;
                case StackItemMode.Hug:
                    if (child.Measure != null) {
                        return child.Measure(availableWidth);
                    }

                    return child.PreferredHeight ?? 0f;
                default:
                    return 0f;
            }
        }

        (float left, float top, float right, float bottom) ResolvePaddingPixels() {
            Style s = node.GetResolvedStyle();
            EdgeInsets pad = s.Padding ?? EdgeInsets.Zero;
            return pad.Resolve(RenderContext.Current.Direction);
        }

        node.MeasureWidth = () => {
            (float left, _, float right, _) = ResolvePaddingPixels();
            float max = 0f;
            for (int i = 0; i < count; i++) {
                LightweaveNode child = builder.Items[i].node;
                if (!child.IsInFlow()) {
                    continue;
                }
                float w = child.MeasureWidth?.Invoke() ?? 0f;
                if (w > max) {
                    max = w;
                }
            }
            return max + left + right;
        };

        bool anyFlex = false;
        for (int i = 0; i < count; i++) {
            if (builder.Items[i].mode == StackItemMode.Flex) {
                anyFlex = true;
                break;
            }
        }

        if (!anyFlex) {
            node.Measure = availableWidth => {
                (float left, float top, float right, float bottom) = ResolvePaddingPixels();
                float innerWidth = Mathf.Max(0f, availableWidth - left - right);
                float total = 0f;
                int flowCount = 0;
                for (int i = 0; i < count; i++) {
                    if (!builder.Items[i].node.IsInFlow()) {
                        continue;
                    }
                    total += ResolveItemHeight(i, innerWidth);
                    flowCount++;
                }

                if (flowCount > 1) {
                    total += ResolveGapPx() * (flowCount - 1);
                }

                return total + top + bottom;
            };
        }

        node.Paint = (rect, paintChildren) => {
            if (count == 0) {
                return;
            }

            float gapPx = ResolveGapPx();
            float nonFlexTotal = 0f;
            int flexCount = 0;
            int flowCount = 0;
            float[] resolvedHeights = new float[count];
            bool[] inFlow = new bool[count];
            for (int i = 0; i < count; i++) {
                bool flow = builder.Items[i].node.IsInFlow();
                inFlow[i] = flow;
                if (!flow) {
                    resolvedHeights[i] = 0f;
                    continue;
                }
                flowCount++;
                if (builder.Items[i].mode == StackItemMode.Flex) {
                    flexCount++;
                    resolvedHeights[i] = 0f;
                }
                else {
                    float h = ResolveItemHeight(i, rect.width);
                    resolvedHeights[i] = h;
                    nonFlexTotal += h;
                }
            }

            float totalGap = gapPx * Math.Max(0, flowCount - 1);
            float remainingForFlex = Mathf.Max(0f, rect.height - nonFlexTotal - totalGap);
            float flexEach = flexCount > 0 ? remainingForFlex / flexCount : 0f;

            float y = rect.y;
            for (int i = 0; i < count; i++) {
                LightweaveNode child = builder.Items[i].node;
                if (!inFlow[i]) {
                    continue;
                }
                float measured = builder.Items[i].mode == StackItemMode.Flex ? flexEach : resolvedHeights[i];
                child.MeasuredRect = new Rect(rect.x, y, rect.width, measured);
                y += measured + gapPx;
            }

            paintChildren();
        };
        return node;
    }

    [DocVariant("CL_Playground_Label_Tight")]
    public static DocSample DocsTight() {
        return new DocSample(() => 
            Stack.Create(
                SpacingScale.Xxs,
                s => {
                    s.Add(SampleChip("A"), new Rem(1.75f).ToPixels());
                    s.Add(SampleChip("B"), new Rem(1.75f).ToPixels());
                    s.Add(SampleChip("C"), new Rem(1.75f).ToPixels());
                }
            )
        );
    }

    [DocVariant("CL_Playground_Label_Loose")]
    public static DocSample DocsLoose() {
        return new DocSample(() => 
            Stack.Create(
                SpacingScale.Sm,
                s => {
                    s.Add(SampleChip("A"), new Rem(2f).ToPixels());
                    s.Add(SampleChip("B"), new Rem(2f).ToPixels());
                    s.Add(SampleChip("C"), new Rem(2f).ToPixels());
                }
            )
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => 
            Stack.Create(
                SpacingScale.Xs,
                s => {
                    s.Add(SampleChip("first"), new Rem(2f).ToPixels());
                    s.Add(SampleChip("second"), new Rem(2f).ToPixels());
                }
            )
        );
    }
}
