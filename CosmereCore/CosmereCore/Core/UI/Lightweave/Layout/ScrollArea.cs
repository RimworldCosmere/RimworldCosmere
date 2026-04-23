using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Lightweave.Hooks;
using Cosmere.Core.UI.Lightweave.Runtime;

namespace Cosmere.Core.UI.Lightweave.Layout;

public static partial class Layout
{
    public static LightweaveNode ScrollArea(
        LightweaveNode content,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        Hooks.Hooks.RefHandle<ScrollViewStatus> statusRef = Hooks.Hooks.UseRef(new ScrollViewStatus(), line, file);

        LightweaveNode node = NodeBuilder.New("ScrollArea", line, file);
        node.Children.Add(content);
        node.Paint = (rect, paintChildren) =>
        {
            float contentHeight = content.PreferredHeight ?? rect.height;
            statusRef.Current.height = contentHeight;
            using (new ScrollView(rect, statusRef.Current))
            {
                float scrollbarGutter = statusRef.Current.scrollVisibile ? 20f : 0f;
                float innerWidth = rect.width - scrollbarGutter;
                content.MeasuredRect = new Rect(0f, 0f, innerWidth, contentHeight);
                paintChildren();
            }
        };
        return node;
    }
}
