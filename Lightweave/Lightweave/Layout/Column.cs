using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using static Cosmere.Lightweave.Playground.PlaygroundChips;

namespace Cosmere.Lightweave.Layout;

public static partial class Layout {
    [Doc(
        Id = "column",
        Summary = "Vertical flow that hugs intrinsic child heights when possible.",
        WhenToUse = "Stack content vertically with consistent spacing and natural heights.",
        SourcePath = "CosmereCore/CosmereCore/Lightweave/Layout/Column.cs",
        PreferredVariantHeight = 160f
    )]
    public static class Column {
        public static LightweaveNode Create(
            [DocParam("Gap between rows.", TypeOverride = "Rem", DefaultOverride = "0")]
            Rem gap = default,
            [DocParam("Cross-axis alignment of children.")]
            FlexAlign align = FlexAlign.Start,
            [DocParam("Main-axis distribution.")]
            FlexJustify justify = FlexJustify.Start,
            [DocParam("Builder callback to populate children.")]
            Action<List<LightweaveNode>>? children = null,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            List<LightweaveNode> kids = new List<LightweaveNode>();
            children?.Invoke(kids);
            LightweaveNode node = NodeBuilder.New("Column", line, file);
            node.Children.AddRange(kids);

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

                    total += gap.ToPixels() * (count - 1);
                    return total;
                };
            }

            node.Paint = (rect, paintChildren) => {
                float gapPx = gap.ToPixels();
                int count = kids.Count;
                if (count == 0) {
                    return;
                }

                bool useIntrinsic = AllKidsKnown();
                float y = rect.y;

                if (useIntrinsic) {
                    for (int i = 0; i < count; i++) {
                        LightweaveNode child = kids[i];
                        float h = ChildHeight(child, rect.width);
                        child.MeasuredRect = new Rect(rect.x, y, rect.width, h);
                        y += h + gapPx;
                    }
                } else {
                    float eachH = (rect.height - gapPx * (count - 1)) / count;
                    for (int i = 0; i < count; i++) {
                        LightweaveNode child = kids[i];
                        child.MeasuredRect = new Rect(rect.x, y, rect.width, eachH);
                        y += eachH + gapPx;
                    }
                }

                paintChildren();
            };
            return node;
        }

        [DocVariant("CC_Playground_Label_Default")]
        public static DocSample DocsDefault() {
            return new DocSample(
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

        [DocUsage]
        public static DocSample DocsUsage() {
            return new DocSample(
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
}
