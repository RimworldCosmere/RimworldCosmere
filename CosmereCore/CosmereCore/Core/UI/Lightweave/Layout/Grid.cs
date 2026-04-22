using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Layout;

public static partial class Layout
{
    public static LightweaveNode Grid(
        IReadOnlyList<GridTrack> columns,
        Rem gap = default,
        Action<List<LightweaveNode>>? children = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        List<GridTrack> cols = new List<GridTrack>(GridTrack.Expand(columns));
        List<LightweaveNode> kids = new List<LightweaveNode>();
        children?.Invoke(kids);
        LightweaveNode node = NodeBuilder.New("Grid", line, file);
        node.Children.AddRange(kids);
        node.Paint = (rect, paintChildren) =>
        {
            Direction dir = RenderContext.Current.Direction;
            float gapPx = gap.ToPixels();
            int n = cols.Count;
            if (n == 0)
            {
                return;
            }
            float totalFixed = 0f;
            float totalFr = 0f;
            float[] widths = new float[n];
            for (int i = 0; i < n; i++)
            {
                if (cols[i] is GridTrack.Fixed f)
                {
                    widths[i] = f.Size.ToPixels();
                    totalFixed += widths[i];
                }
                else if (cols[i] is GridTrack.Fr fr)
                {
                    totalFr += fr.Weight;
                }
            }
            float frAvailable = rect.width - totalFixed - gapPx * Math.Max(0, n - 1);
            for (int i = 0; i < n; i++)
            {
                if (cols[i] is GridTrack.Fr fr && totalFr > 0)
                {
                    widths[i] = frAvailable * (fr.Weight / totalFr);
                }
            }

            int childIdx = 0;
            float y = rect.y;
            float rowHeight = rect.height;
            while (childIdx < kids.Count)
            {
                float x = dir == Direction.Rtl ? rect.xMax : rect.x;
                for (int i = 0; i < n && childIdx < kids.Count; i++)
                {
                    LightweaveNode child = kids[childIdx++];
                    float w = widths[i];
                    Rect childRect = dir == Direction.Ltr
                        ? new Rect(x, y, w, rowHeight)
                        : new Rect(x - w, y, w, rowHeight);
                    child.MeasuredRect = childRect;
                    if (dir == Direction.Ltr)
                    {
                        x += w + gapPx;
                    }
                    else
                    {
                        x -= w + gapPx;
                    }
                }
                y += rowHeight + gapPx;
            }
            paintChildren();
        };
        return node;
    }
}
