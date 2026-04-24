using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Core.UI.Lightweave.Layout;

public static partial class Layout {
    public static LightweaveNode Container(
        LightweaveNode child,
        Rem maxWidth = default,
        EdgeInsets? padding = null,
        ContainerAlign align = ContainerAlign.Center,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("Container", line, file);
        node.Children.Add(child);

        float maxWidthPx = maxWidth.ToPixels();
        EdgeInsets pad = padding ?? EdgeInsets.Zero;

        float ResolveInnerWidth(float availableWidth) {
            float outer = maxWidthPx > 0f ? Mathf.Min(availableWidth, maxWidthPx) : availableWidth;
            (float leftPx, float topPx, float rightPx, float bottomPx) = pad.Resolve(RenderContext.Current.Direction);
            return Mathf.Max(0f, outer - leftPx - rightPx);
        }

        node.Measure = availableWidth => {
            float innerWidth = ResolveInnerWidth(availableWidth);
            (float leftPx, float topPx, float rightPx, float bottomPx) = pad.Resolve(RenderContext.Current.Direction);
            float childHeight = child.Measure?.Invoke(innerWidth) ?? child.PreferredHeight ?? 0f;
            return childHeight + topPx + bottomPx;
        };

        node.Paint = (rect, paintChildren) => {
            Direction dir = RenderContext.Current.Direction;
            float outer = maxWidthPx > 0f ? Mathf.Min(rect.width, maxWidthPx) : rect.width;
            float offsetX = align switch {
                ContainerAlign.Start => dir == Direction.Rtl ? rect.width - outer : 0f,
                ContainerAlign.End => dir == Direction.Rtl ? 0f : rect.width - outer,
                _ => (rect.width - outer) * 0.5f,
            };
            Rect outerRect = new Rect(rect.x + offsetX, rect.y, outer, rect.height);
            Rect inner = pad.Shrink(outerRect, dir);
            child.MeasuredRect = inner;
            paintChildren();
        };

        return node;
    }
}

public enum ContainerAlign {
    Start,
    Center,
    End,
}
