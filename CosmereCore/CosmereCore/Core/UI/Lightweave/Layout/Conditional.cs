using System;
using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Runtime;

namespace Cosmere.Core.UI.Lightweave.Layout;

public static partial class Layout
{
    public static LightweaveNode Conditional(
        bool when,
        Func<LightweaveNode> children,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        LightweaveNode n = NodeBuilder.New($"Conditional({when})", line, file);
        if (when)
        {
            LightweaveNode child = children();
            n.Children.Add(child);
            n.Paint = rect => child.MeasuredRect = rect;
        }
        else
        {
            n.Paint = _ => { };
        }
        return n;
    }
}
