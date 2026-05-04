using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Types;

namespace Cosmere.Lightweave.Doc;

public static partial class Doc {
    public static LightweaveNode DirectionScope(
        Direction direction,
        LightweaveNode child,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New($"Doc.DirectionScope:{direction}", line, file);
        node.Children.Add(child);
        node.Measure = availableWidth => {
            RenderContext.Current.DirectionStack.Push(direction);
            try {
                return child.Measure?.Invoke(availableWidth) ?? child.PreferredHeight ?? 0f;
            }
            finally {
                RenderContext.Current.DirectionStack.Pop();
            }
        };
        node.Paint = (rect, _) => {
            RenderContext.Current.DirectionStack.Push(direction);
            try {
                child.MeasuredRect = rect;
                LightweaveRoot.PaintSubtree(child, rect);
            }
            finally {
                RenderContext.Current.DirectionStack.Pop();
            }
        };
        return node;
    }
}
