using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Layout;

public static partial class Layout
{
    public static LightweaveNode Column(
        Rem gap = default,
        FlexAlign align = FlexAlign.Start,
        FlexJustify justify = FlexJustify.Start,
        Action<List<LightweaveNode>>? children = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        List<LightweaveNode> kids = new List<LightweaveNode>();
        children?.Invoke(kids);
        LightweaveNode node = NodeBuilder.New("Column", line, file);
        node.Children.AddRange(kids);
        node.Paint = (rect, paintChildren) =>
        {
            float gapPx = gap.ToPixels();
            int count = kids.Count;
            if (count == 0)
            {
                return;
            }
            float y = rect.y;
            float eachH = (rect.height - gapPx * (count - 1)) / count;
            for (int i = 0; i < count; i++)
            {
                LightweaveNode child = kids[i];
                Rect childRect = new Rect(rect.x, y, rect.width, eachH);
                child.MeasuredRect = childRect;
                y += eachH + gapPx;
            }
            paintChildren();
        };
        return node;
    }
}
