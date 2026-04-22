using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Runtime;

namespace Cosmere.Core.UI.Lightweave.Layout;

public static partial class Layout
{
    public static LightweaveNode Stack(
        Action<List<LightweaveNode>>? children = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        List<LightweaveNode> kids = new List<LightweaveNode>();
        children?.Invoke(kids);
        LightweaveNode node = NodeBuilder.New("Stack", line, file);
        node.Children.AddRange(kids);
        node.Paint = (rect, paintChildren) =>
        {
            foreach (LightweaveNode child in kids)
            {
                child.MeasuredRect = rect;
            }
            paintChildren();
        };
        return node;
    }
}
