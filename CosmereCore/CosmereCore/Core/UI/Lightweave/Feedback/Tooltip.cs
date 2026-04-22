using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Feedback;

public static class Tooltip
{
    private const float HoverDelaySeconds = 0.5f;

    public static LightweaveNode Wrap(
        LightweaveNode children,
        LightweaveNode content,
        Vector2? preferredSize = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        LightweaveNode node = NodeBuilder.New("Tooltip", line, file);
        node.Children.Add(children);

        Hooks.Hooks.RefHandle<float> hoverTimer = Hooks.Hooks.UseRef<float>(0f, line, file);

        node.Paint = (rect, _) =>
        {
            children.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(children, rect);

            bool hovered = Mouse.IsOver(rect);
            Event e = Event.current;

            if (!hovered || e.type == EventType.MouseDown)
            {
                hoverTimer.Current = 0f;
                return;
            }

            if (e.type == EventType.Repaint)
            {
                hoverTimer.Current += Time.unscaledDeltaTime;
            }
            if (hoverTimer.Current < HoverDelaySeconds)
            {
                return;
            }

            Vector2 size = preferredSize ?? new Vector2(new Rem(20f).ToPixels(), new Rem(6f).ToPixels());
            Vector2 mouse = e.mousePosition;
            float offsetX = 12f;
            float offsetY = 18f;
            float x = mouse.x + offsetX;
            float y = mouse.y + offsetY;

            if (x + size.x > Screen.width)
            {
                x = mouse.x - offsetX - size.x;
            }
            if (y + size.y > Screen.height)
            {
                y = mouse.y - offsetY - size.y;
            }
            if (x < 0f)
            {
                x = 0f;
            }
            if (y < 0f)
            {
                y = 0f;
            }

            Rect tooltipRect = new Rect(x, y, size.x, size.y);

            RenderContext.Current.PendingOverlays.Enqueue(() =>
            {
                BackgroundSpec bg = new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised);
                BorderSpec border = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault);
                RadiusSpec radius = RadiusSpec.All(new Rem(0.25f));
                PaintBox.Draw(tooltipRect, bg, border, radius);

                float pad = new Rem(0.5f).ToPixels();
                Rect innerRect = new Rect(
                    tooltipRect.x + pad,
                    tooltipRect.y + pad,
                    tooltipRect.width - pad * 2f,
                    tooltipRect.height - pad * 2f);

                LightweaveRoot.PaintSubtree(content, innerRect);
            });
        };

        return node;
    }
}
