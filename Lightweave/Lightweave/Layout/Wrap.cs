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
    Id = "wrap",
    Summary = "Wrapping flow that re-flows children onto new rows when the line fills.",
    WhenToUse = "Variable count of equally-sized chips that should wrap to width.",
    SourcePath = "Lightweave/Lightweave/Layout/Wrap.cs",
    PreferredVariantHeight = 120f
)]
public static class Wrap {
    public static LightweaveNode Create(
        [DocParam("Gap between cells.", TypeOverride = "Rem", DefaultOverride = "0")]
        Rem gap = default,
        [DocParam("Minimum width per cell. Drives how many fit per row.", TypeOverride = "Rem", DefaultOverride = "0")]
        Rem minChildWidth = default,
        [DocParam("Builder callback to populate children.")]
        Action<List<LightweaveNode>>? children = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        List<LightweaveNode> kids = new List<LightweaveNode>();
        children?.Invoke(kids);
        LightweaveNode node = NodeBuilder.New("Wrap", line, file);
        node.Children.AddRange(kids);

        node.Measure = availableWidth => {
            if (kids.Count == 0) {
                return 0f;
            }

            float gapPx = gap.ToPixels();
            float minW = Mathf.Max(minChildWidth.ToPixels(), 1f);
            float rowH = minW * 0.6f;
            int perRow = Mathf.Max(1, Mathf.FloorToInt((availableWidth + gapPx) / (minW + gapPx)));
            int rows = (kids.Count + perRow - 1) / perRow;
            return rows * rowH + Mathf.Max(0, rows - 1) * gapPx;
        };

        node.Paint = (rect, paintChildren) => {
            float gapPx = gap.ToPixels();
            float minW = Mathf.Max(minChildWidth.ToPixels(), 1f);
            float x = rect.x;
            float y = rect.y;
            float rowH = minW * 0.6f;
            foreach (LightweaveNode child in kids) {
                if (x + minW > rect.xMax) {
                    x = rect.x;
                    y += rowH + gapPx;
                }

                child.MeasuredRect = new Rect(x, y, minW, rowH);
                x += minW + gapPx;
            }

            paintChildren();
        };
        return node;
    }

    [DocVariant("CC_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(
            Wrap.Create(
                SpacingScale.Xs,
                new Rem(3f),
                k => {
                    k.Add(SampleChip("one"));
                    k.Add(SampleChip("two"));
                    k.Add(SampleChip("three"));
                    k.Add(SampleChip("four"));
                }
            )
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(
            Wrap.Create(
                SpacingScale.Xs,
                new Rem(3f),
                k => {
                    k.Add(SampleChip("alpha"));
                    k.Add(SampleChip("beta"));
                    k.Add(SampleChip("gamma"));
                }
            )
        );
    }
}
