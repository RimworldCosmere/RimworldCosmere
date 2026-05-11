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
        [DocParam("Gap between rows and columns.")]
        Rem gap = default,
        [DocParam("Builder callback to populate cells in row-major order.")]
        Action<List<LightweaveNode>>? children = null,
        [DocParam("Inline style override.", TypeOverride = "Style?", DefaultOverride = "null")]
        Style? style = null,
        [DocParam("Additional class names merged after the base 'grid' class.", TypeOverride = "string[]?", DefaultOverride = "null")]
        string[]? classes = null,
        [DocParam("Stable id for state-style lookup.", TypeOverride = "string?", DefaultOverride = "null")]
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        List<LightweaveNode> kids = new List<LightweaveNode>();
        children?.Invoke(kids);
        LightweaveNode node = NodeBuilder.New("Grid", line, file);
        node.ApplyStyling("grid", style, classes, id);
        node.Children.AddRange(kids);

        List<GridTrack> ResolveCols() {
            return new List<GridTrack>(GridTrack.Expand(columns));
        }

        float[] ResolveColumnWidths(float availableWidth, List<GridTrack> cols, float gapPx) {
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

        List<LightweaveNode> CollectInFlow() {
            List<LightweaveNode> flow = new List<LightweaveNode>(kids.Count);
            for (int i = 0; i < kids.Count; i++) {
                if (kids[i].IsInFlow()) {
                    flow.Add(kids[i]);
                }
            }
            return flow;
        }

        node.Measure = availableWidth => {
            List<GridTrack> cols = ResolveCols();
            int n = cols.Count;
            List<LightweaveNode> flow = CollectInFlow();
            if (n == 0 || flow.Count == 0) {
                return 0f;
            }

            float gapPx = gap.ToPixels();
            float[] widths = ResolveColumnWidths(availableWidth, cols, gapPx);
            int rows = (flow.Count + n - 1) / n;
            float totalHeight = 0f;
            for (int r = 0; r < rows; r++) {
                float rowMax = 0f;
                for (int c = 0; c < n; c++) {
                    int idx = r * n + c;
                    if (idx >= flow.Count) {
                        break;
                    }

                    LightweaveNode child = flow[idx];
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
            List<GridTrack> cols = ResolveCols();
            int n = cols.Count;
            if (n == 0) {
                return;
            }

            List<LightweaveNode> flow = CollectInFlow();
            float gapPx = gap.ToPixels();
            Direction dir = RenderContext.Current.Direction;
            float[] widths = ResolveColumnWidths(rect.width, cols, gapPx);

            int childIdx = 0;
            float y = rect.y;
            float rowHeight = rect.height;
            while (childIdx < flow.Count) {
                float x = dir == Direction.Rtl ? rect.xMax : rect.x;
                for (int i = 0; i < n && childIdx < flow.Count; i++) {
                    LightweaveNode child = flow[childIdx++];
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

    [DocVariant("CL_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(() => 
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


    [DocVariant("CL_Playground_Grid_Responsive")]
    public static DocSample DocsResponsive() {
        IReadOnlyList<GridTrack> oneCol = new List<GridTrack> {
            new GridTrack.Fr(1f),
        };
        IReadOnlyList<GridTrack> twoCol = new List<GridTrack> {
            new GridTrack.Fr(1f),
            new GridTrack.Fr(1f),
        };
        IReadOnlyList<GridTrack> fourCol = new List<GridTrack> {
            new GridTrack.Fr(1f),
            new GridTrack.Fr(1f),
            new GridTrack.Fr(1f),
            new GridTrack.Fr(1f),
        };
        return new DocSample(() =>
            Grid.Create(
                Breakpoints.PickRef(oneCol, md: twoCol, lg: fourCol),
                SpacingScale.Xs,
                k => {
                    k.Add(SampleChip("1"));
                    k.Add(SampleChip("2"));
                    k.Add(SampleChip("3"));
                    k.Add(SampleChip("4"));
                }
            ),
            useFullSource: true
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => 
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
