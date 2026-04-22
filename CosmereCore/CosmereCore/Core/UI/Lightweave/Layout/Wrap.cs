using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Layout;

public static partial class Layout
{
    public static LightweaveNode Wrap(
        Rem gap = default,
        Rem minChildWidth = default,
        Action<List<LightweaveNode>>? children = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        List<LightweaveNode> kids = new List<LightweaveNode>();
        children?.Invoke(kids);
        LightweaveNode node = NodeBuilder.New("Wrap", line, file);
        node.Children.AddRange(kids);
        node.Paint = rect =>
        {
            float gapPx = gap.ToPixels();
            float minW = Mathf.Max(minChildWidth.ToPixels(), 1f);
            float x = rect.x;
            float y = rect.y;
            float rowH = minW * 0.6f;
            foreach (LightweaveNode child in kids)
            {
                if (x + minW > rect.xMax)
                {
                    x = rect.x;
                    y += rowH + gapPx;
                }
                child.MeasuredRect = new Rect(x, y, minW, rowH);
                x += minW + gapPx;
            }
        };
        return node;
    }
}
