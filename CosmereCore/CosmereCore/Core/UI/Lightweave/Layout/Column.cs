using System;
using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Core.UI.Lightweave.Layout;

public static partial class Layout {
    public static LightweaveNode Column(
        Rem gap = default,
        FlexAlign align = FlexAlign.Start,
        FlexJustify justify = FlexJustify.Start,
        Action<List<LightweaveNode>>? children = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        List<LightweaveNode> kids = new List<LightweaveNode>();
        children?.Invoke(kids);
        LightweaveNode node = NodeBuilder.New("Column", line, file);
        node.Children.AddRange(kids);

        bool AllKidsKnown() {
            for (int i = 0; i < kids.Count; i++) {
                if (kids[i].Measure == null && !kids[i].PreferredHeight.HasValue) {
                    return false;
                }
            }

            return true;
        }

        float ChildHeight(LightweaveNode child, float width) {
            return child.Measure?.Invoke(width) ?? child.PreferredHeight ?? 0f;
        }

        if (AllKidsKnown()) {
            node.Measure = width => {
                int count = kids.Count;
                if (count == 0) {
                    return 0f;
                }

                float total = 0f;
                for (int i = 0; i < count; i++) {
                    total += ChildHeight(kids[i], width);
                }

                total += gap.ToPixels() * (count - 1);
                return total;
            };
        }

        node.Paint = (rect, paintChildren) => {
            float gapPx = gap.ToPixels();
            int count = kids.Count;
            if (count == 0) {
                return;
            }

            bool useIntrinsic = AllKidsKnown();
            float y = rect.y;

            if (useIntrinsic) {
                for (int i = 0; i < count; i++) {
                    LightweaveNode child = kids[i];
                    float h = ChildHeight(child, rect.width);
                    child.MeasuredRect = new Rect(rect.x, y, rect.width, h);
                    y += h + gapPx;
                }
            } else {
                float eachH = (rect.height - gapPx * (count - 1)) / count;
                for (int i = 0; i < count; i++) {
                    LightweaveNode child = kids[i];
                    child.MeasuredRect = new Rect(rect.x, y, rect.width, eachH);
                    y += eachH + gapPx;
                }
            }

            paintChildren();
        };
        return node;
    }
}