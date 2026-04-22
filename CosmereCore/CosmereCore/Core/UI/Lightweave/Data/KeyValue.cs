using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Data;

public static class KeyValue
{
    public static LightweaveNode Create(
        string label,
        LightweaveNode value,
        Rem? labelWidth = null,
        [CallerFilePath] string? caller = null,
        [CallerLineNumber] int line = 0)
    {
        LightweaveNode labelNode = Typography.Typography.Text(
            label,
            font: new FontRef.Role(FontRole.Label));

        LightweaveNode node = NodeBuilder.New("KeyValue", line, caller ?? string.Empty);
        node.Children.Add(labelNode);
        node.Children.Add(value);

        node.Paint = (rect, paintChildren) =>
        {
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;
            float lw = (labelWidth ?? new Rem(8f)).ToPixels();

            Rect labelRect;
            Rect valueRect;

            if (rtl)
            {
                labelRect = new Rect(rect.xMax - lw, rect.y, lw, rect.height);
                valueRect = new Rect(rect.x, rect.y, Mathf.Max(0f, rect.width - lw), rect.height);
            }
            else
            {
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
