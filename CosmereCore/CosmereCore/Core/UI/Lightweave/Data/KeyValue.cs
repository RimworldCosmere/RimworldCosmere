using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Core.UI.Lightweave.Data;

public static class KeyValue {
    public static LightweaveNode Create(
        string label,
        LightweaveNode value,
        Rem? labelWidth = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode labelNode = Typography.Typography.Text(
            label,
            new FontRef.Role(FontRole.Label)
        );

        LightweaveNode node = NodeBuilder.New("KeyValue", line, file);
        node.Children.Add(labelNode);
        node.Children.Add(value);

        node.Measure = availableWidth => {
            float lw = (labelWidth ?? new Rem(8f)).ToPixels();
            float valueWidth = Mathf.Max(0f, availableWidth - lw);
            float labelH = labelNode.Measure?.Invoke(lw) ?? labelNode.PreferredHeight ?? 0f;
            float valueH = value.Measure?.Invoke(valueWidth) ?? value.PreferredHeight ?? 0f;
            return Mathf.Max(labelH, valueH);
        };

        node.Paint = (rect, paintChildren) => {
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;
            float lw = (labelWidth ?? new Rem(8f)).ToPixels();

            Rect labelRect;
            Rect valueRect;

            if (rtl) {
                labelRect = new Rect(rect.xMax - lw, rect.y, lw, rect.height);
                valueRect = new Rect(rect.x, rect.y, Mathf.Max(0f, rect.width - lw), rect.height);
            } else {
                labelRect = new Rect(rect.x, rect.y, lw, rect.height);
                valueRect = new Rect(rect.x + lw, rect.y, Mathf.Max(0f, rect.width - lw), rect.height);
            }

            labelNode.MeasuredRect = labelRect;
            value.MeasuredRect = valueRect;

            paintChildren();
        };

        return node;
    }
}