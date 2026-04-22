using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Layout;

public static partial class Layout
{
    public static class Spacer
    {
        public static LightweaveNode Flex(
            int weight = 1,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = "")
        {
            LightweaveNode n = NodeBuilder.New($"Spacer.Flex({weight})", line, file);
            n.Paint = (_, _) => { };
            return n;
        }

        public static LightweaveNode Fixed(
            Rem size,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = "")
        {
            LightweaveNode n = NodeBuilder.New($"Spacer.Fixed({size.Value})", line, file);
            n.Paint = (_, _) => { };
            return n;
        }
    }
}
