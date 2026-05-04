using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Lightweave.Playground;

public static class PlaygroundShell {
    public static LightweaveNode Create(
        LightweaveNode header,
        LightweaveNode rail,
        LightweaveNode body,
        float headerHeight = 56f,
        float railWidth = 200f,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("PlaygroundShell", line, file);
        node.Children.Add(header);
        node.Children.Add(rail);
        node.Children.Add(body);

        node.Paint = (rect, paintChildren) => {
            Direction dir = RenderContext.Current.Direction;
            Theme.Theme theme = RenderContext.Current.Theme;
            bool rtl = dir == Direction.Rtl;

            float resolvedHeaderHeight = header.Measure?.Invoke(rect.width) ?? header.PreferredHeight ?? headerHeight;
            Rect headerRect = new Rect(rect.x, rect.y, rect.width, resolvedHeaderHeight);

            float rowY = rect.y + resolvedHeaderHeight;
            float rowHeight = rect.height - resolvedHeaderHeight;
            Rect rowRect = new Rect(rect.x, rowY, rect.width, rowHeight);

            Rect railRect;
            Rect bodyRect;
            if (rtl) {
                railRect = new Rect(rowRect.xMax - railWidth, rowRect.y, railWidth, rowRect.height);
                bodyRect = new Rect(rowRect.x, rowRect.y, rowRect.width - railWidth, rowRect.height);
            }
            else {
                railRect = new Rect(rowRect.x, rowRect.y, railWidth, rowRect.height);
                bodyRect = new Rect(rowRect.x + railWidth, rowRect.y, rowRect.width - railWidth, rowRect.height);
            }

            float bodyLeadingPad = SpacingScale.Md.ToPixels();
            float bodyTrailingPad = 0f;
            float bodyBottomMargin = 0f;
            if (rtl) {
                bodyRect = new Rect(
                    bodyRect.x + bodyTrailingPad,
                    bodyRect.y,
                    bodyRect.width - bodyLeadingPad - bodyTrailingPad,
                    bodyRect.height - bodyBottomMargin
                );
            }
            else {
                bodyRect = new Rect(
                    bodyRect.x + bodyLeadingPad,
                    bodyRect.y,
                    bodyRect.width - bodyLeadingPad - bodyTrailingPad,
                    bodyRect.height - bodyBottomMargin
                );
            }

            header.MeasuredRect = headerRect;
            rail.MeasuredRect = railRect;
            body.MeasuredRect = bodyRect;

            paintChildren();

            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.BorderSubtle);

            Rect headerDivider = new Rect(rect.x, rect.y + resolvedHeaderHeight - 1f, rect.width, 1f);
            GUI.DrawTexture(headerDivider, Texture2D.whiteTexture);

            float dividerX = rtl ? railRect.x : railRect.xMax - 1f;
            Rect railDivider = new Rect(dividerX, rowRect.y, 1f, rowRect.height);
            GUI.DrawTexture(railDivider, Texture2D.whiteTexture);

            GUI.color = saved;
        };

        return node;
    }
}