using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Runtime;

namespace Cosmere.Core.UI.Lightweave.Layout;

public static partial class Layout
{
    public static LightweaveNode Each<T>(
        IEnumerable<T> items,
        Func<T, int, LightweaveNode> render,
        Func<T, object>? keyFn = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        LightweaveNode n = NodeBuilder.New("Each", line, file);
        int i = 0;
        foreach (T item in items)
        {
            LightweaveNode child = render(item, i);
            child.ExplicitKey = keyFn?.Invoke(item);
            n.Children.Add(child);
            i++;
        }
        n.Paint = (rect, paintChildren) =>
        {
            foreach (LightweaveNode child in n.Children)
            {
                child.MeasuredRect = rect;
            }
            paintChildren();
        };
        return n;
    }
}
