using System.Runtime.CompilerServices;

namespace Cosmere.Lightweave.Runtime;

public static class NodeBuilder {
    public static LightweaveNode New(
        string debugName,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        unchecked {
            int hash = 17;
            hash = hash * 31 + (file?.GetHashCode() ?? 0);
            hash = hash * 31 + line;
            int parentHash = RenderContext.CurrentOrNull?.ParentPathHash ?? 0;
            return new LightweaveNode {
                CallSiteId = hash,
                BuildParentPathHash = parentHash,
                DebugName = debugName,
            };
        }
    }
}