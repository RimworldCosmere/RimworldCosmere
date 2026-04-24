using System;
using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Core.UI.Lightweave.Layout;

public static partial class Layout {
    public static LightweaveNode Wrap(
        Rem gap = default,
        Rem minChildWidth = default,
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
}