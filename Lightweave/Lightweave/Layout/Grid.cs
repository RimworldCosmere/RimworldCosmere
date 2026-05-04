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
    Id = "grid",
    Summary = "Two-dimensional column-driven layout with fixed and fractional tracks.",
    WhenToUse = "Place children on a column grid with explicit track sizes.",
    SourcePath = "Lightweave/Lightweave/Layout/Grid.cs"
)]
public static class Grid {
    public static LightweaveNode Create(
        [DocParam("Column track specs (Fixed or Fr).")]
        IReadOnlyList<GridTrack> columns,
        [DocParam("Gap between rows and columns.", TypeOverride = "Rem", DefaultOverride = "0")]
        Rem gap = default,
        [DocParam("Builder callback to populate cells in row-major order.")]
        Action<List<LightweaveNode>>? children = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        List<GridTrack> cols = new List<GridTrack>(GridTrack.Expand(columns));
        List<LightweaveNode> kids = new List<LightweaveNode>();
        children?.Invoke(kids);
        LightweaveNode node = NodeBuilder.New("Grid", line, file);
        node.Children.AddRange(kids);

        float[] ResolveColumnWidths(float availableWidth) {
            float gapPx = gap.ToPixels();
            int n = cols.Count;
            float[] widths = new float[n];
            float totalFixed = 0f;
            float totalFr = 0f;
            for (int i = 0; i < n; i++) {
                if (cols[i] is GridTrack.Fixed f) {
                    widths[i] = f.Size.ToPixels();
                    totalFixed += widths[i];
                }
                else if (cols[i] is GridTrack.Fr fr) {
                    totalFr += fr.Weight;
                }
            }

            float frAvailable = availableWidth - totalFixed - gapPx * Math.Max(0, n - 1);
            for (int i = 0; i < n; i++) {
                if (cols[i] is GridTrack.Fr fr && totalFr > 0) {
                    widths[i] = frAvailable * (fr.Weight / totalFr);
                }
            }

            return widths;
        }

        node.Measure = availableWidth => {
            int n = cols.Count;
            if (n == 0 || kids.Count == 0) {
                return 0f;
            }

            float[] widths = ResolveColumnWidths(availableWidth);
            int rows = (kids.Count + n - 1) / n;
            float totalHeight = 0f;
            float gapPx = gap.ToPixels();
            for (int r = 0; r < rows; r++) {
                float rowMax = 0f;
                for (int c = 0; c < n; c++) {
                    int idx = r * n + c;
                    if (idx >= kids.Count) {
                        break;
                    }

                    LightweaveNode child = kids[idx];
                    float h = child.Measure?.Invoke(widths[c]) ?? child.PreferredHeight ?? 0f;
                    if (h > rowMax) {
                        rowMax = h;
                    }
                }

                totalHeight += rowMax;
            }

            totalHeight += gapPx * Math.Max(0, rows - 1);
            return totalHeight;
        };

        node.Paint = (rect, paintChildren) => {
            Direction dir = RenderContext.Current.Direction;
            float gapPx = gap.ToPixels();
            int n = cols.Count;
            if (n == 0) {
                return;
            }

            float[] widths = ResolveColumnWidths(rect.width);

            int childIdx = 0;
            float y = rect.y;
            float rowHeight = rect.height;
            while (childIdx < kids.Count) {
                float x = dir == Direction.Rtl ? rect.xMax : rect.x;
                for (int i = 0; i < n && childIdx < kids.Count; i++) {
                    LightweaveNode child = kids[childIdx++];
                    float w = widths[i];
                    Rect childRect = dir == Direction.Ltr
                        ? new Rect(x, y, w, rowHeight)
                        : new Rect(x - w, y, w, rowHeight);
                    child.MeasuredRect = childRect;
                    if (dir == Direction.Ltr) {
                        x += w + gapPx;
                    }
                    else {
                        x -= w + gapPx;
                    }
                }

                y += rowHeight + gapPx;
            }

            paintChildren();
        };
        return node;
    }

    [DocVariant("CC_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(
            Grid.Create(
                new List<GridTrack> {
                    new GridTrack.Fr(1f),
                    new GridTrack.Fr(1f),
                    new GridTrack.Fr(1f),
                },
                SpacingScale.Xs,
                k => {
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
            Grid.Create(
                new List<GridTrack> {
                    new GridTrack.Fr(1f),
                    new GridTrack.Fr(1f),
                },
                SpacingScale.Xs,
                k => {
                    k.Add(SampleChip("left"));
                    k.Add(SampleChip("right"));
                }
            )
        );
    }
}
