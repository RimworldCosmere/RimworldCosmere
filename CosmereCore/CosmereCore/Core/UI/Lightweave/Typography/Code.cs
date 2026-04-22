using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Typography;

public static partial class Typography
{
    public static LightweaveNode Code(
        string text,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "") =>
        Text(text, FontRole.Mono, new Rem(0.875f), ThemeSlot.TextPrimary, TextAlign.Start, FontStyle.Normal, line, file);
}
