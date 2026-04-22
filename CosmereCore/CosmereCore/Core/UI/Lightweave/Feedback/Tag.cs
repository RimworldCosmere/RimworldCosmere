using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Input;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Feedback;

public static class Tag
{
    public static LightweaveNode Create(
        string text,
        Action? onDismiss = null,
        BadgeVariant variant = BadgeVariant.Neutral,
        LightweaveNode? leading = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        if (onDismiss == null)
        {
            return Badge.Create(text, variant, leading, line, file);
        }

        LightweaveNode node = NodeBuilder.New($"Tag:{variant}", line, file);

        if (leading != null)
        {
            node.Children.Add(leading);
        }

        LightweaveNode dismissIcon = BuildDismissGlyph(variant, line, file);
        LightweaveNode dismissButton = IconButton.Create(
            icon: dismissIcon,
            onClick: onDismiss,
            variant: ButtonVariant.Ghost,
            disabled: false,
            tooltipKey: null,
            caller: file,
            line: line);
        node.Children.Add(dismissButton);

        node.Paint = (rect, paintChildren) =>
        {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            BackgroundSpec bg = new BackgroundSpec.Solid(BadgeVariants.Background(variant));
            BorderSpec border = BorderSpec.All(new Rem(1f / 16f), BadgeVariants.Border(variant));
            RadiusSpec radius = RadiusSpec.All(new Rem(999f));

            PaintBox.Draw(rect, bg, border, radius);

            float padPx = new Rem(0.5f).ToPixels();
            float gapPx = new Rem(0.25f).ToPixels();
            float dismissGap = new Rem(0.125f).ToPixels();
            float iconSize = new Rem(0.875f).ToPixels();
            float dismissSize = new Rem(1f).ToPixels();

            float dismissX = rtl
                ? rect.x + padPx
                : rect.xMax - padPx - dismissSize;
            Rect dismissRect = new Rect(dismissX, rect.y + (rect.height - dismissSize) / 2f, dismissSize, dismissSize);
            dismissButton.MeasuredRect = dismissRect;

            Rect textRect;
            if (rtl)
            {
                float startX = dismissRect.xMax + dismissGap;
                textRect = new Rect(startX, rect.y, rect.xMax - padPx - startX, rect.height);
            }
            else
            {
                textRect = new Rect(rect.x + padPx, rect.y, dismissRect.x - dismissGap - (rect.x + padPx), rect.height);
            }

            if (leading != null)
            {
                float iconX = rtl
                    ? textRect.xMax - iconSize
                    : textRect.x;
                Rect iconRect = new Rect(iconX, rect.y + (rect.height - iconSize) / 2f, iconSize, iconSize);
                leading.MeasuredRect = iconRect;
                if (rtl)
                {
                    textRect = new Rect(textRect.x, textRect.y, iconRect.x - gapPx - textRect.x, textRect.height);
                }
                else
                {
                    float newX = iconRect.xMax + gapPx;
                    textRect = new Rect(newX, textRect.y, textRect.xMax - newX, textRect.height);
                }
            }

            Font font = theme.GetFont(FontRole.BodyBold);
            int pixelSize = Mathf.RoundToInt(new Rem(0.75f).ToPixels());
            GUIStyle style = GuiStyleCache.Get(font, pixelSize, FontStyle.Bold);
            style.alignment = TextAnchor.MiddleCenter;

            // NOTE: Small-caps is approximated via uppercasing; fine for Latin, may need revisiting for non-Latin scripts.
            string display = text.ToUpperInvariant();

            Color savedColor = GUI.color;
            GUI.color = theme.GetColor(BadgeVariants.Foreground(variant));
            GUI.Label(RectSnap.Snap(textRect), display, style);
            GUI.color = savedColor;

            paintChildren();
        };

        return node;
    }

    private static LightweaveNode BuildDismissGlyph(BadgeVariant variant, int line, string file)
    {
        LightweaveNode glyph = NodeBuilder.New("TagDismissIcon", line, file);
        glyph.Paint = (rect, _) =>
        {
            Theme.Theme theme = RenderContext.Current.Theme;
            Font font = theme.GetFont(FontRole.BodyBold);
            int pixelSize = Mathf.RoundToInt(new Rem(0.75f).ToPixels());
            GUIStyle style = GuiStyleCache.Get(font, pixelSize, FontStyle.Bold);
            style.alignment = TextAnchor.MiddleCenter;

            Color savedColor = GUI.color;
            GUI.color = theme.GetColor(BadgeVariants.Foreground(variant));
            GUI.Label(RectSnap.Snap(rect), "×", style);
            GUI.color = savedColor;
        };
        return glyph;
    }
}
