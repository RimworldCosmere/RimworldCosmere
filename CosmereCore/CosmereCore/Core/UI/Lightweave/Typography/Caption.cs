using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Typography;

public static partial class Typography {
    public static LightweaveNode Caption(
        string text,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        return Text(
            text,
            FontRole.Caption,
            new Rem(0.75f),
            ThemeSlot.TextMuted,
            line: line,
            file: file
        );
    }
}