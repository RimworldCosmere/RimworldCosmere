using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Typography;

public static partial class Typography
{
    public static LightweaveNode RichText(
        TaggedString content,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        LightweaveNode node = NodeBuilder.New("RichText", line, file);
        node.Paint = rect =>
        {
            Theme.Theme theme = RenderContext.Current.Theme;
            Font f = theme.GetFont(FontRole.Body);
            GUIStyle style = GuiStyleCache.Get(f, 16, FontStyle.Normal);
            style.richText = true;
            style.alignment = ResolveAnchor(TextAlign.Start, RenderContext.Current.Direction);
            GUI.Label(RectSnap.Snap(rect), content.Resolve(), style);
        };
        return node;
    }
}
