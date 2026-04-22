using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Typography;

public static partial class Typography
{
    public static LightweaveNode Heading(
        int level,
        string text,
        ColorRef? color = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        Rem size = level switch
        {
            1 => new Rem(2f),
            2 => new Rem(1.5f),
            3 => new Rem(1.25f),
            _ => new Rem(1.125f),
        };
        return Text(text, FontRole.Heading, size, color, TextAlign.Start, FontStyle.Bold, line, file);
    }
}
