using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Typography;

public static partial class Typography
{
    public static LightweaveNode Text(
        string content,
        FontRef? font = null,
        Rem? size = null,
        ColorRef? color = null,
        TextAlign align = TextAlign.Start,
        FontStyle weight = FontStyle.Normal,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        LightweaveNode node = NodeBuilder.New($"Text:{content}", line, file);
        node.Paint = (rect, _) =>
        {
            Theme.Theme theme = RenderContext.Current.Theme;
            Font f = font switch
            {
                FontRef.Literal lit => lit.Value,
                FontRef.Role role => theme.GetFont(role.RoleValue),
                _ => theme.GetFont(FontRole.Body),
            };
            int pixelSize = Mathf.RoundToInt((size ?? new Rem(1f)).ToPixels());
            GUIStyle style = GuiStyleCache.Get(f, pixelSize, weight);
            style.alignment = ResolveAnchor(align, RenderContext.Current.Direction);
            Color c = color switch
            {
                ColorRef.Literal lit => lit.Value,
                ColorRef.Token tok   => theme.GetColor(tok.Slot),
                _ => theme.GetColor(ThemeSlot.TextPrimary),
            };
            Color saved = GUI.color;
            GUI.color = c;
            GUI.Label(RectSnap.Snap(rect), content, style);
            GUI.color = saved;
        };
        return node;
    }
}
