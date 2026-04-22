using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Overlay;

public static class Popover
{
    public static LightweaveNode Create(
        bool isOpen,
        Rect anchorRect,
        PopoverPlacement placement,
        LightweaveNode content,
        Action onDismiss,
        Vector2? preferredSize = null,
        [CallerFilePath] string? caller = null,
        [CallerLineNumber] int line = 0)
    {
        if (!isOpen)
        {
            LightweaveNode empty = NodeBuilder.New("Popover:closed", line, caller ?? string.Empty);
            empty.Paint = (_, _) => { };
            return empty;
        }

        LightweaveNode node = NodeBuilder.New($"Popover:{placement}", line, caller ?? string.Empty);
        node.Paint = (rect, paintChildren) =>
        {
            Vector2 size = preferredSize ?? new Vector2(new Rem(15f).ToPixels(), new Rem(10f).ToPixels());
            Rect screen = new Rect(0f, 0f, Screen.width, Screen.height);
            Direction dir = RenderContext.Current.Direction;
            Rect popoverRect = PopoverLayout.Resolve(anchorRect, placement, dir, size, screen);

            RenderContext.Current.PendingOverlays.Enqueue(() =>
            {
                BackgroundSpec bg = new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised);
                BorderSpec border = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault);
                RadiusSpec radius = RadiusSpec.All(new Rem(0.5f));
                PaintBox.Draw(popoverRect, bg, border, radius);

                LightweaveRoot.PaintSubtree(content, popoverRect);

                Event e = Event.current;
                if (e.type == EventType.MouseDown
                    && !popoverRect.Contains(e.mousePosition)
                    && !anchorRect.Contains(e.mousePosition))
                {
                    onDismiss?.Invoke();
                    e.Use();
                }
            });
        };
        return node;
    }
}
