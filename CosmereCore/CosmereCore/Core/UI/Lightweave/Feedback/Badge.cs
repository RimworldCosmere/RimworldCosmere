using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Feedback;

public static class Badge
{
    public static LightweaveNode Create(
        string text,
        BadgeVariant variant = BadgeVariant.Neutral,
        LightweaveNode? leading = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        LightweaveNode node = NodeBuilder.New($"Badge:{variant}", line, file);

        if (leading != null)
        {
            node.Children.Add(leading);
        }

        // NOTE: Small-caps is approximated via uppercasing; fine for Latin, may need revisiting for non-Latin scripts.
        string display = text.ToUpperInvariant();

        node.Paint = (rect, paintChildren) =>
        {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            BackgroundSpec bg = new BackgroundSpec.Solid(BadgeVariants.Background(variant));
            ThemeSlot? borderSlot = BadgeVariants.Border(variant);
            BorderSpec? border = borderSlot.HasValue
                ? (BorderSpec?)BorderSpec.All(new Rem(1f / 16f), borderSlot.Value)
                : null;
            RadiusSpec radius = RadiusSpec.All(new Rem(999f));

            PaintBox.Draw(rect, bg, border, radius);

            float padPx = new Rem(0.5f).ToPixels();
            float gapPx = new Rem(0.25f).ToPixels();
            float iconSize = new Rem(0.875f).ToPixels();

            Rect textRect = new Rect(rect.x + padPx, rect.y, rect.width - padPx * 2f, rect.height);

            if (leading != null)
            {
                float iconX = rtl
                    ? rect.xMax - padPx - iconSize
                    : rect.x + padPx;
                Rect iconRect = new Rect(iconX, rect.y + (rect.height - iconSize) / 2f, iconSize, iconSize);
                leading.MeasuredRect = iconRect;
                if (rtl)
                {
                    textRect = new Rect(textRect.x, textRect.y, iconX - gapPx - textRect.x, textRect.height);
                }
                else
                {
                    float newX = iconX + iconSize + gapPx;
                    textRect = new Rect(newX, textRect.y, rect.xMax - padPx - newX, textRect.height);
                }
            }

            Font font = theme.GetFont(FontRole.BodyBold);
            int pixelSize = Mathf.RoundToInt(new Rem(0.75f).ToPixels());
            GUIStyle style = GuiStyleCache.Get(font, pixelSize, FontStyle.Bold);
            style.alignment = TextAnchor.MiddleCenter;

            Color savedColor = GUI.color;
            GUI.color = theme.GetColor(BadgeVariants.Foreground(variant));
            GUI.Label(RectSnap.Snap(textRect), display, style);
            GUI.color = savedColor;

            paintChildren();
        };

        return node;
    }
}
