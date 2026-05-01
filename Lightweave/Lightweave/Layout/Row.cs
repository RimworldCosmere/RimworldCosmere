using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using static Cosmere.Lightweave.Doc.DocChips;

namespace Cosmere.Lightweave.Layout;

public static partial class Layout {
    [Doc(
        Id = "row",
        Summary = "Horizontal flow that splits available width evenly across children.",
        WhenToUse = "Lay out a fixed set of peers side by side.",
        SourcePath = "CosmereCore/CosmereCore/Lightweave/Layout/Row.cs"
    )]
    public static class Row {
        public static LightweaveNode Create(
            [DocParam("Gap between columns.", TypeOverride = "Rem", DefaultOverride = "0")]
            Rem gap = default,
            [DocParam("Cross-axis alignment of children.")]
            FlexAlign align = FlexAlign.Start,
            [DocParam("Builder callback to populate children.")]
            Action<List<LightweaveNode>>? children = null,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            List<LightweaveNode> kids = new List<LightweaveNode>();
            children?.Invoke(kids);
            LightweaveNode node = NodeBuilder.New("Row", line, file);
            node.Children.AddRange(kids);

            node.Measure = availableWidth => {
                int count = kids.Count;
                if (count == 0) {
                    return 0f;
                }

                float gapPx = gap.ToPixels();
                float eachW = (availableWidth - gapPx * Math.Max(0, count - 1)) / count;
                float maxH = 0f;
                for (int i = 0; i < count; i++) {
                    float h = kids[i].Measure?.Invoke(eachW) ?? kids[i].PreferredHeight ?? 0f;
                    if (h > maxH) {
                        maxH = h;
                    }
                }

                return maxH;
            };

            node.Paint = (rect, paintChildren) => {
                Direction dir = RenderContext.Current.Direction;
                bool reverse = dir == Direction.Rtl;
                float gapPx = gap.ToPixels();
                List<LightweaveNode> seq = reverse ? ReversedList(kids) : kids;
                int count = seq.Count;
                if (count == 0) {
                    return;
                }

                float available = rect.width - gapPx * Math.Max(0, count - 1);
                float eachW = available / count;
                float x = rect.x;
                for (int i = 0; i < count; i++) {
                    LightweaveNode child = seq[i];
                    Rect childRect = new Rect(x, rect.y, eachW, rect.height);
                    child.MeasuredRect = childRect;
                    x += eachW + gapPx;
                }

                paintChildren();
            };
            return node;
        }

        private static List<LightweaveNode> ReversedList(List<LightweaveNode> src) {
            List<LightweaveNode> r = new List<LightweaveNode>(src.Count);
            for (int i = src.Count - 1; i >= 0; i--) {
                r.Add(src[i]);
            }

            return r;
        }

        [DocVariant("CC_Playground_Label_Default")]
        public static DocSample DocsDefault() {
            return new DocSample(
                Row.Create(
                    SpacingScale.Xs,
                    children: k => {
                        k.Add(SampleChip("A"));
                        k.Add(SampleChip("B"));
                        k.Add(SampleChip("C"));
                    }
                )
            );
        }

        [DocUsage]
        public static DocSample DocsUsage() {
            return new DocSample(
                Row.Create(
                    SpacingScale.Xs,
                    children: k => {
                        k.Add(SampleChip("first"));
                        k.Add(SampleChip("second"));
                    }
                )
            );
        }
    }
}
