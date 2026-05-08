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
        [DocParam("Gap between stacked items. Accepts a Responsive<Rem> for breakpoint-driven gaps.", TypeOverride = "Responsive<Rem>", DefaultOverride = "0")]
        Responsive<Rem> gap = default,
        [DocParam("Builder callback to populate items via Add / AddFlex.")]
        Action<StackBuilder>? children = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        StackBuilder builder = new StackBuilder();
        children?.Invoke(builder);

        LightweaveNode node = NodeBuilder.New("Stack", line, file);
        int count = builder.Items.Count;
        for (int i = 0; i < count; i++) {
            node.Children.Add(builder.Items[i].node);
        }

        float ResolveGapPx() {
            return gap.Resolve(RenderContext.Current.Breakpoint).ToPixels();
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

        bool anyFlex = false;
        for (int i = 0; i < count; i++) {
            if (builder.Items[i].mode == StackItemMode.Flex) {
                anyFlex = true;
                break;
            }
        }

        if (!anyFlex) {
            node.Measure = availableWidth => {
                float total = 0f;
                for (int i = 0; i < count; i++) {
                    total += ResolveItemHeight(i, availableWidth);
                }

                if (count > 1) {
                    total += ResolveGapPx() * (count - 1);
                }

                return total;
            };
        }

        node.Paint = (rect, paintChildren) => {
            if (count == 0) {
                return;
            }

            float gapPx = ResolveGapPx();
            float nonFlexTotal = 0f;
            int flexCount = 0;
            float[] resolvedHeights = new float[count];
            for (int i = 0; i < count; i++) {
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

            float totalGap = gapPx * Math.Max(0, count - 1);
            float remainingForFlex = Mathf.Max(0f, rect.height - nonFlexTotal - totalGap);
            float flexEach = flexCount > 0 ? remainingForFlex / flexCount : 0f;

            float y = rect.y;
            for (int i = 0; i < count; i++) {
                LightweaveNode child = builder.Items[i].node;
                float measured = builder.Items[i].mode == StackItemMode.Flex ? flexEach : resolvedHeights[i];
                child.MeasuredRect = new Rect(rect.x, y, rect.width, measured);
                y += measured + gapPx;
            }

            paintChildren();
        };
        return node;
    }

    [DocVariant("CC_Playground_Label_Tight")]
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

    [DocVariant("CC_Playground_Label_Loose")]
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
