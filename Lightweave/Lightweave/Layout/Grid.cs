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
        [DocParam("Column track specs (Fixed or Fr). Accepts a Responsive<IReadOnlyList<GridTrack>> for breakpoint-driven column counts.", TypeOverride = "Responsive<IReadOnlyList<GridTrack>>")]
        Responsive<IReadOnlyList<GridTrack>> columns,
        [DocParam("Gap between rows and columns. Accepts a Responsive<Rem> for breakpoint-driven gaps.", TypeOverride = "Responsive<Rem>", DefaultOverride = "0")]
        Responsive<Rem> gap = default,
        [DocParam("Builder callback to populate cells in row-major order.")]
        Action<List<LightweaveNode>>? children = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        List<LightweaveNode> kids = new List<LightweaveNode>();
        children?.Invoke(kids);
        LightweaveNode node = NodeBuilder.New("Grid", line, file);
        node.Children.AddRange(kids);

        List<GridTrack> ResolveCols() {
            return new List<GridTrack>(GridTrack.Expand(columns.Resolve(RenderContext.Current.Breakpoint)));
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

        node.Measure = availableWidth => {
            List<GridTrack> cols = ResolveCols();
            int n = cols.Count;
            if (n == 0 || kids.Count == 0) {
                return 0f;
            }

            float gapPx = gap.Resolve(RenderContext.Current.Breakpoint).ToPixels();
            float[] widths = ResolveColumnWidths(availableWidth, cols, gapPx);
            int rows = (kids.Count + n - 1) / n;
            float totalHeight = 0f;
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
            List<GridTrack> cols = ResolveCols();
            int n = cols.Count;
            if (n == 0) {
                return;
            }

            float gapPx = gap.Resolve(RenderContext.Current.Breakpoint).ToPixels();
            Direction dir = RenderContext.Current.Direction;
            float[] widths = ResolveColumnWidths(rect.width, cols, gapPx);

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


    [DocVariant("CC_Playground_Grid_Responsive")]
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
        Responsive<IReadOnlyList<GridTrack>> columns = Responsive.From(
            oneCol,
            (Breakpoint.Md, twoCol),
            (Breakpoint.Lg, fourCol)
        );
        return new DocSample(
            Grid.Create(
                columns,
                SpacingScale.Xs,
                k => {
                    k.Add(SampleChip("1"));
                    k.Add(SampleChip("2"));
                    k.Add(SampleChip("3"));
                    k.Add(SampleChip("4"));
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
