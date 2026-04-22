using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Typography;

public static partial class Typography
{
    public static LightweaveNode Icon(
        Texture texture,
        Rem? size = null,
        ColorRef? color = null,
        bool mirrorInRtl = false,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        LightweaveNode node = NodeBuilder.New("Icon", line, file);
        node.Paint = (rect, _) =>
        {
            Theme.Theme theme = RenderContext.Current.Theme;
            float px = (size ?? new Rem(1.5f)).ToPixels();
            Rect r = new Rect(rect.x, rect.y + (rect.height - px) / 2f, px, px);
            Color c = color switch
            {
                ColorRef.Literal lit => lit.Value,
                ColorRef.Token tok => theme.GetColor(tok.Slot),
                _ => Color.white,
            };
            Matrix4x4 saved = default;
            bool pushed = false;
            if (mirrorInRtl)
            {
                saved = IconMirror.PushIfRtl(r, RenderContext.Current.Direction);
                pushed = true;
            }
            Color savedColor = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(RectSnap.Snap(r), texture, ScaleMode.ScaleToFit);
            GUI.color = savedColor;
            if (pushed)
            {
                IconMirror.Pop(saved);
            }
        };
        return node;
    }
}
