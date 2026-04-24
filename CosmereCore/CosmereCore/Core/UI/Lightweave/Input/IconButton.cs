using System;
using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Core.UI.Lightweave.Input;

public static class IconButton {
    public static LightweaveNode Create(
        LightweaveNode icon,
        Action? onClick,
        ButtonVariant variant = ButtonVariant.Ghost,
        Rem? iconSize = null,
        bool disabled = false,
        string? tooltipKey = null,
        [CallerFilePath] string? caller = null,
        [CallerLineNumber] int line = 0
    ) {
        _ = tooltipKey;

        LightweaveNode node = NodeBuilder.New("IconButton", line, caller ?? string.Empty);
        node.Children.Add(icon);

        float iconPx = (iconSize ?? new Rem(1.25f)).ToPixels();
        float padPx = SpacingScale.Xs.ToPixels();
        float squareSize = iconPx + padPx * 2f;
        node.PreferredHeight = squareSize;

        node.Paint = (rect, paintChildren) => {
            float size = Mathf.Min(squareSize, Mathf.Min(rect.width, rect.height));
            Rect square = new Rect(
                rect.x + (rect.width - size) / 2f,
                rect.y + (rect.height - size) / 2f,
                size,
                size
            );

            InteractionState state = InteractionState.Resolve(square, null, disabled);

            ThemeSlot bgSlot = ButtonVariants.Background(variant, state);
            ThemeSlot? borderSlot = ButtonVariants.Border(variant, state);

            BackgroundSpec bg = new BackgroundSpec.Solid(bgSlot);
            BorderSpec? border = borderSlot.HasValue
                ? BorderSpec.All(new Rem(1f / 16f), borderSlot.Value)
                : null;
            RadiusSpec radius = RadiusSpec.All(new Rem(0.25f));

            PaintBox.Draw(square, bg, border, radius);

            float overlay = ButtonVariants.OverlayAlpha(state);
            if (overlay > 0f) {
                Color overlayColor = state.Pressed
                    ? new Color(0f, 0f, 0f, overlay)
                    : new Color(1f, 1f, 1f, overlay);
                PaintBox.Draw(square, new BackgroundSpec.Solid(overlayColor), null, radius);
            }

            float innerPad = Mathf.Min(padPx, (size - iconPx) / 2f);
            Rect padRect = new Rect(
                square.x + innerPad,
                square.y + innerPad,
                size - innerPad * 2f,
                size - innerPad * 2f
            );
            icon.MeasuredRect = padRect;

            paintChildren();

            Event e = Event.current;
            if (!disabled &&
                onClick != null &&
                e.type == EventType.MouseUp &&
                e.button == 0 &&
                square.Contains(e.mousePosition)) {
                onClick.Invoke();
                e.Use();
            }
        };

        return node;
    }
}