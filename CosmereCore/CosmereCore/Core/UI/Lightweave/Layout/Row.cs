using System;
using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Core.UI.Lightweave.Layout;

public static partial class Layout {
    public static LightweaveNode Row(
        Rem gap = default,
        FlexAlign align = FlexAlign.Start,
        FlexJustify justify = FlexJustify.Start,
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
            float remaining = 0f;
            float startX = rect.x;
            if (justify == FlexJustify.End) {
                startX += remaining;
            } else if (justify == FlexJustify.Center) {
                startX += remaining / 2f;
            }

            float x = startX;
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
}