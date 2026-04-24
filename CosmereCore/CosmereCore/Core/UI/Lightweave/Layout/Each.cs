using System;
using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Core.UI.Lightweave.Layout;

public static partial class Layout {
    public static LightweaveNode Each<T>(
        IEnumerable<T> items,
        Func<T, int, LightweaveNode> render,
        Func<T, object>? keyFn = null,
        Rem? gap = null,
        EachOrientation orientation = EachOrientation.Horizontal,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode n = NodeBuilder.New("Each", line, file);
        int i = 0;
        foreach (T item in items) {
            LightweaveNode child = render(item, i);
            child.ExplicitKey = keyFn?.Invoke(item);
            n.Children.Add(child);
            i++;
        }

        float gapPx = (gap ?? SpacingScale.Xs).ToPixels();

        if (orientation == EachOrientation.Horizontal) {
            n.Measure = availableWidth => {
                int count = n.Children.Count;
                if (count == 0) {
                    return 0f;
                }

                float totalGap = gapPx * Mathf.Max(0, count - 1);
                float childW = Mathf.Max(0f, (availableWidth - totalGap) / count);
                float maxH = 0f;
                foreach (LightweaveNode child in n.Children) {
                    float h = child.Measure?.Invoke(childW) ?? child.PreferredHeight ?? 0f;
                    if (h > maxH) {
                        maxH = h;
                    }
                }

                return maxH;
            };
            n.Paint = (rect, paintChildren) => {
                int count = n.Children.Count;
                if (count == 0) {
                    return;
                }

                bool rtl = RenderContext.Current.Direction == Direction.Rtl;
                float totalGap = gapPx * Mathf.Max(0, count - 1);
                float childW = Mathf.Max(0f, (rect.width - totalGap) / count);
                float cursor = rtl ? rect.xMax - childW : rect.x;
                for (int j = 0; j < count; j++) {
                    n.Children[j].MeasuredRect = new Rect(cursor, rect.y, childW, rect.height);
                    cursor += rtl ? -(childW + gapPx) : childW + gapPx;
                }

                paintChildren();
            };
        } else {
            n.Measure = availableWidth => {
                int count = n.Children.Count;
                if (count == 0) {
                    return 0f;
                }

                float totalGap = gapPx * Mathf.Max(0, count - 1);
                float total = 0f;
                foreach (LightweaveNode child in n.Children) {
                    total += child.Measure?.Invoke(availableWidth) ?? child.PreferredHeight ?? 0f;
                }

                return total + totalGap;
            };
            n.Paint = (rect, paintChildren) => {
                int count = n.Children.Count;
                if (count == 0) {
                    return;
                }

                float y = rect.y;
                for (int j = 0; j < count; j++) {
                    LightweaveNode child = n.Children[j];
                    float h = child.Measure?.Invoke(rect.width) ?? child.PreferredHeight ?? 0f;
                    child.MeasuredRect = new Rect(rect.x, y, rect.width, h);
                    y += h + gapPx;
                }

                paintChildren();
            };
        }

        return n;
    }
}

public enum EachOrientation {
    Horizontal,
    Vertical,
}