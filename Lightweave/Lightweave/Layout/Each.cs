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
    Id = "each",
    Summary = "Render a node per item in a sequence with optional key extractor.",
    WhenToUse = "Iterate a collection into a flat list of nodes for a Row/Column/HStack/Grid.",
    SourcePath = "Lightweave/Lightweave/Layout/Each.cs"
)]
public static class Each {
    public static LightweaveNode Of<T>(
        [DocParam("Source items.")]
        IEnumerable<T> items,
        [DocParam("Render callback receiving the item and its index.")]
        Func<T, int, LightweaveNode> render,
        [DocParam("Optional stable key extractor for diffing.", TypeOverride = "Func<T, object>?", DefaultOverride = "null")]
        Func<T, object>? keyFn = null,
        [DocParam("Gap between siblings.", TypeOverride = "Rem?", DefaultOverride = "SpacingScale.Xs")]
        Rem? gap = null,
        [DocParam("Layout axis: Horizontal or Vertical.")]
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
        }
        else {
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

    [DocVariant("CC_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        string[] items = new[] { "A", "B", "C" };
        return new DocSample(
            Row.Create(
                SpacingScale.Xs,
                children: r => {
                    r.Add(
                        Each.Of(
                            items,
                            (item, _) => SampleChip(item),
                            item => item
                        )
                    );
                }
            )
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        string[] items = new[] { "alpha", "beta", "gamma" };
        return new DocSample(
            Each.Of(items, (item, _) => SampleChip(item))
        );
    }
}

public enum EachOrientation {
    Horizontal,
    Vertical,
}
