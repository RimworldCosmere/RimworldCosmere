using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Input;

public static class IconButton
{
    public static LightweaveNode Create(
        LightweaveNode icon,
        Action? onClick,
        ButtonVariant variant = ButtonVariant.Ghost,
        bool disabled = false,
        string? tooltipKey = null,
        [CallerFilePath] string? caller = null,
        [CallerLineNumber] int line = 0)
    {
        _ = tooltipKey;

        LightweaveNode node = NodeBuilder.New("IconButton", line, caller ?? string.Empty);
        node.Children.Add(icon);

        node.Paint = (rect, paintChildren) =>
        {
            InteractionState state = InteractionState.Resolve(rect, focusName: null, disabled: disabled);

            ThemeSlot bgSlot = ButtonVariants.Background(variant, state);
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

            float padPx = SpacingScale.Xs.ToPixels();
            Rect padRect = new Rect(rect.x + padPx, rect.y + padPx, rect.width - padPx * 2f, rect.height - padPx * 2f);
            icon.MeasuredRect = padRect;

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
