using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Playground;

public static class PlaygroundShell
{
    public static LightweaveNode Create(
        LightweaveNode header,
        LightweaveNode rail,
        LightweaveNode body,
        float headerHeight = 56f,
        float railWidth = 200f,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        LightweaveNode node = NodeBuilder.New("PlaygroundShell", line, file);
        node.Children.Add(header);
        node.Children.Add(rail);
        node.Children.Add(body);

        node.Paint = (rect, paintChildren) =>
        {
            Direction dir = RenderContext.Current.Direction;
            Theme.Theme theme = RenderContext.Current.Theme;
            bool rtl = dir == Direction.Rtl;

            Rect headerRect = new Rect(rect.x, rect.y, rect.width, headerHeight);

            float rowY = rect.y + headerHeight;
            float rowHeight = rect.height - headerHeight;
            Rect rowRect = new Rect(rect.x, rowY, rect.width, rowHeight);

            Rect railRect;
            Rect bodyRect;
            if (rtl)
            {
                railRect = new Rect(rowRect.xMax - railWidth, rowRect.y, railWidth, rowRect.height);
                bodyRect = new Rect(rowRect.x, rowRect.y, rowRect.width - railWidth, rowRect.height);
            }
            else
            {
                railRect = new Rect(rowRect.x, rowRect.y, railWidth, rowRect.height);
                bodyRect = new Rect(rowRect.x + railWidth, rowRect.y, rowRect.width - railWidth, rowRect.height);
            }

            header.MeasuredRect = headerRect;
            rail.MeasuredRect = railRect;
            body.MeasuredRect = bodyRect;

            paintChildren();

            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.BorderSubtle);

            Rect headerDivider = new Rect(rect.x, rect.y + headerHeight - 1f, rect.width, 1f);
            GUI.DrawTexture(headerDivider, Texture2D.whiteTexture);

            float dividerX = rtl ? railRect.x : railRect.xMax - 1f;
            Rect railDivider = new Rect(dividerX, rowRect.y, 1f, rowRect.height);
            GUI.DrawTexture(railDivider, Texture2D.whiteTexture);

            GUI.color = saved;
        };

        return node;
    }
}
