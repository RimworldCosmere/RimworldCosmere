using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Input;

public static class Button
{
    public static LightweaveNode Create(
        string label,
        Action? onClick,
        ButtonVariant variant = ButtonVariant.Primary,
        LightweaveNode? leading = null,
        LightweaveNode? trailing = null,
        ColorRef? foregroundOverride = null,
        bool disabled = false,
        [CallerFilePath] string? caller = null,
        [CallerLineNumber] int line = 0)
    {
        LightweaveNode node = NodeBuilder.New($"Button:{label}", line, caller ?? string.Empty);

        if (leading != null)
        {
            node.Children.Add(leading);
        }
        if (trailing != null)
        {
            node.Children.Add(trailing);
        }

        node.Paint = (rect, paintChildren) =>
        {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            InteractionState state = InteractionState.Resolve(rect, focusName: null, disabled: disabled);

            ThemeSlot bgSlot = ButtonVariants.Background(variant, state);
            ThemeSlot fgSlot = ButtonVariants.Foreground(variant, state);
            ThemeSlot? borderSlot = ButtonVariants.Border(variant, state);

            BackgroundSpec bg = new BackgroundSpec.Solid(bgSlot);
            BorderSpec? border = borderSlot.HasValue
                ? (BorderSpec?)BorderSpec.All(new Rem(1f / 16f), borderSlot.Value)
                : null;
            RadiusSpec radius = RadiusSpec.All(new Rem(0.25f));

            PaintBox.Draw(rect, bg, border, radius);

            float overlay = ButtonVariants.OverlayAlpha(state);
            if (overlay > 0f)
            {
                Color overlayColor = state.Pressed
                    ? new Color(0f, 0f, 0f, overlay)
                    : new Color(1f, 1f, 1f, overlay);
                PaintBox.Draw(rect, new BackgroundSpec.Solid(overlayColor), null, radius);
            }

            float padPx = SpacingScale.Sm.ToPixels();
            float iconSize = Mathf.Min(rect.height - padPx, new Rem(1.25f).ToPixels());
            bool rtl = dir == Direction.Rtl;

            Rect labelRect = new Rect(rect.x + padPx, rect.y, rect.width - padPx * 2f, rect.height);

            if (leading != null)
            {
                float leadingX = rtl
                    ? rect.xMax - padPx - iconSize
                    : rect.x + padPx;
                Rect leadingRect = new Rect(leadingX, rect.y + (rect.height - iconSize) / 2f, iconSize, iconSize);
                leading.MeasuredRect = leadingRect;
                if (rtl)
                {
                    labelRect = new Rect(labelRect.x, labelRect.y, labelRect.width - (iconSize + padPx), labelRect.height);
                }
                else
                {
                    labelRect = new Rect(leadingX + iconSize + padPx, labelRect.y, rect.xMax - padPx - (leadingX + iconSize + padPx), labelRect.height);
                }
            }

            if (trailing != null)
            {
                float trailingX = rtl
                    ? rect.x + padPx
                    : rect.xMax - padPx - iconSize;
                Rect trailingRect = new Rect(trailingX, rect.y + (rect.height - iconSize) / 2f, iconSize, iconSize);
                trailing.MeasuredRect = trailingRect;
                if (rtl)
                {
                    labelRect = new Rect(trailingX + iconSize + padPx, labelRect.y, labelRect.xMax - (trailingX + iconSize + padPx), labelRect.height);
                }
                else
                {
                    labelRect = new Rect(labelRect.x, labelRect.y, trailingX - padPx - labelRect.x, labelRect.height);
                }
            }

            Font font = theme.GetFont(FontRole.BodyBold);
            int pixelSize = Mathf.RoundToInt(new Rem(0.875f).ToPixels());
            GUIStyle style = GuiStyleCache.Get(font, pixelSize, FontStyle.Bold);
            style.alignment = TextAnchor.MiddleCenter;

            Color fg = foregroundOverride switch
            {
                ColorRef.Literal lit => lit.Value,
                ColorRef.Token tok => theme.GetColor(tok.Slot),
                _ => theme.GetColor(fgSlot),
            };

            Color savedColor = GUI.color;
            GUI.color = fg;
            GUI.Label(RectSnap.Snap(labelRect), label, style);
            GUI.color = savedColor;

            paintChildren();

            Event e = Event.current;
            if (!disabled && onClick != null && e.type == EventType.MouseUp && e.button == 0 && rect.Contains(e.mousePosition))
            {
                onClick.Invoke();
                e.Use();
            }
        };

        return node;
    }
}
