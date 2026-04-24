using System;
using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Runtime;

namespace Cosmere.Core.UI.Lightweave.Layout;

public static partial class Layout {
    public static LightweaveNode Conditional(
        bool when,
        Func<LightweaveNode> children,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode n = NodeBuilder.New($"Conditional({when})", line, file);
        if (when) {
            LightweaveNode child = children();
            n.Children.Add(child);
            if (child.Measure != null) {
                n.Measure = child.Measure;
            } else if (child.PreferredHeight.HasValue) {
                n.PreferredHeight = child.PreferredHeight.Value;
            }

            n.Paint = (rect, paintChildren) => {
                child.MeasuredRect = rect;
                paintChildren();
            };
        } else {
            n.PreferredHeight = 0f;
            n.Paint = (_, _) => { };
        }

        return n;
    }
}