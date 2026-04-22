using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Surface;

public static partial class Surface
{
    public static LightweaveNode Box(
        EdgeInsets? padding = null,
        BackgroundSpec? background = null,
        BorderSpec? border = null,
        RadiusSpec? radius = null,
        Action<List<LightweaveNode>>? children = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        List<LightweaveNode> kids = new List<LightweaveNode>();
        children?.Invoke(kids);
        EdgeInsets pad = padding ?? EdgeInsets.Zero;

        LightweaveNode node = NodeBuilder.New("Box", line, file);
        node.Children.AddRange(kids);
        node.Paint = rect =>
        {
            PaintBox.Draw(rect, background, border, radius);
            Rect content = pad.Shrink(rect, RenderContext.Current.Direction);
            foreach (LightweaveNode child in kids)
            {
                child.MeasuredRect = content;
            }
        };
        return node;
    }
}
