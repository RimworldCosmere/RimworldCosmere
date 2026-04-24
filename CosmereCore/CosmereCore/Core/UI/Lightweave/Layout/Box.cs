using System;
using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Core.UI.Lightweave.Layout;

public static partial class Layout {
    public static LightweaveNode Box(
        EdgeInsets? padding = null,
        BackgroundSpec? background = null,
        BorderSpec? border = null,
        RadiusSpec? radius = null,
        Action<List<LightweaveNode>>? children = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        List<LightweaveNode> kids = new List<LightweaveNode>();
        children?.Invoke(kids);
        EdgeInsets pad = padding ?? EdgeInsets.Zero;

        LightweaveNode node = NodeBuilder.New("Box", line, file);
        node.Children.AddRange(kids);

        float ChildMeasure(LightweaveNode child, float width) {
            if (child.Measure != null) {
                return child.Measure(width);
            }

            return child.PreferredHeight ?? 0f;
        }

        bool CanMeasure() {
            int n = kids.Count;
            if (n == 0) {
                return true;
            }

            for (int i = 0; i < n; i++) {
                LightweaveNode k = kids[i];
                if (k.Measure == null && !k.PreferredHeight.HasValue) {
                    return false;
                }
            }

            return true;
        }

        if (CanMeasure()) {
            node.Measure = availableWidth => {
                Direction dir = RenderContext.Current.Direction;
                (float left, float top, float right, float bottom) = pad.Resolve(dir);
                float innerWidth = Mathf.Max(0f, availableWidth - left - right);
                int n = kids.Count;
                float total = 0f;
                for (int i = 0; i < n; i++) {
                    total += ChildMeasure(kids[i], innerWidth);
                }

                return total + top + bottom;
            };
        }

        node.Paint = (rect, paintChildren) => {
            PaintBox.Draw(rect, background, border, radius);
            Rect content = pad.Shrink(rect, RenderContext.Current.Direction);
            int count = kids.Count;
            if (count == 0) {
                return;
            }

            bool anyIntrinsic = false;
            float[] intrinsic = new float[count];
            for (int i = 0; i < count; i++) {
                LightweaveNode k = kids[i];
                if (k.Measure != null || k.PreferredHeight.HasValue) {
                    intrinsic[i] = ChildMeasure(k, content.width);
                    anyIntrinsic = true;
                } else {
                    intrinsic[i] = -1f;
                }
            }

            float y = content.y;
            if (anyIntrinsic) {
                float knownTotal = 0f;
                int unknownCount = 0;
                for (int i = 0; i < count; i++) {
                    if (intrinsic[i] >= 0f) {
                        knownTotal += intrinsic[i];
                    } else {
                        unknownCount++;
                    }
                }

                float remaining = Mathf.Max(0f, content.height - knownTotal);
                float unknownEach = unknownCount > 0 ? remaining / unknownCount : 0f;
                for (int i = 0; i < count; i++) {
                    LightweaveNode child = kids[i];
                    float h = intrinsic[i] >= 0f ? intrinsic[i] : unknownEach;
                    child.MeasuredRect = new Rect(content.x, y, content.width, h);
                    y += h;
                }
            } else {
                float eachH = content.height / count;
                for (int i = 0; i < count; i++) {
                    LightweaveNode child = kids[i];
                    child.MeasuredRect = new Rect(content.x, y, content.width, eachH);
                    y += eachH;
                }
            }

            paintChildren();
        };
        return node;
    }
}
