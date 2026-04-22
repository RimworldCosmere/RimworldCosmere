using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Lightweave.Hooks;
using Cosmere.Core.UI.Lightweave.Runtime;

namespace Cosmere.Core.UI.Lightweave.Layout;

public static partial class Layout
{
    public static LightweaveNode ScrollArea(
        float contentHeight,
        Action<List<LightweaveNode>> children,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        List<LightweaveNode> kids = new List<LightweaveNode>();
        children(kids);
        Hooks.Hooks.RefHandle<ScrollViewStatus> statusRef = Hooks.Hooks.UseRef(new ScrollViewStatus(), line, file);

        LightweaveNode node = NodeBuilder.New("ScrollArea", line, file);
        node.Children.AddRange(kids);
        node.Paint = rect =>
        {
            statusRef.Current.height = contentHeight;
            using (new ScrollView(rect, statusRef.Current))
            {
                float y = 0f;
                float scrollbarGutter = statusRef.Current.scrollVisibile ? 20f : 0f;
                float innerWidth = rect.width - scrollbarGutter;
                foreach (LightweaveNode child in kids)
                {
                    Rect childRect = new Rect(0, y, innerWidth, 32f);
                    child.MeasuredRect = childRect;
                    y += 32f;
                }
            }
        };
        return node;
    }
}
