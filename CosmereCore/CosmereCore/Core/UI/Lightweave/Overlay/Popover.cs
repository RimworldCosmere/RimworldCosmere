using System;
using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Core.UI.Lightweave.Overlay;

public static class Popover {
    public static LightweaveNode Create(
        bool isOpen,
        Rect anchorRect,
        PopoverPlacement placement,
        LightweaveNode content,
        Action onDismiss,
        Vector2? preferredSize = null,
        [CallerFilePath] string? caller = null,
        [CallerLineNumber] int line = 0
    ) {
        if (!isOpen) {
            LightweaveNode empty = NodeBuilder.New("Popover:closed", line, caller ?? string.Empty);
            empty.Paint = (_, _) => { };
            return empty;
        }

        LightweaveNode node = NodeBuilder.New($"Popover:{placement}", line, caller ?? string.Empty);
        node.Paint = (rect, paintChildren) => {
            Vector2 size = preferredSize ?? new Vector2(new Rem(15f).ToPixels(), -1f);
            if (size.x <= 0f) {
                size.x = new Rem(15f).ToPixels();
            }

            if (size.y <= 0f) {
                float measuredHeight = content.Measure?.Invoke(size.x) ??
                                       content.PreferredHeight ?? new Rem(10f).ToPixels();
                size.y = measuredHeight;
            }

            Rect screen = RenderContext.Current.RootRect;
            Direction dir = RenderContext.Current.Direction;

            Vector2 anchorTopLeft = GUIUtility.GUIToScreenPoint(new Vector2(anchorRect.x, anchorRect.y));
            Rect anchorAbsolute = new Rect(anchorTopLeft.x, anchorTopLeft.y, anchorRect.width, anchorRect.height);

            RenderContext.Current.PendingOverlays.Enqueue(() => {
                    Vector2 anchorLocal = GUIUtility.ScreenToGUIPoint(new Vector2(anchorAbsolute.x, anchorAbsolute.y));
                    Rect anchorHere = new Rect(
                        anchorLocal.x,
                        anchorLocal.y,
                        anchorAbsolute.width,
                        anchorAbsolute.height
                    );
                    Rect popoverRect = PopoverLayout.Resolve(anchorHere, placement, dir, size, screen);

                    Color savedColor = GUI.color;
                    GUI.color = Color.white;

                    Rect shadowRect = new Rect(
                        popoverRect.x + 2f,
                        popoverRect.y + 3f,
                        popoverRect.width,
                        popoverRect.height
                    );
                    BackgroundSpec shadowBg = new BackgroundSpec.Solid(new Color(0f, 0f, 0f, 0.35f));
                    PaintBox.Draw(shadowRect, shadowBg, null, RadiusSpec.All(new Rem(0.5f)));

                    BackgroundSpec bg = new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised);
                    BorderSpec border = BorderSpec.All(new Rem(2f / 16f), ThemeSlot.BorderDefault);
                    RadiusSpec radius = RadiusSpec.All(new Rem(0.5f));
                    PaintBox.Draw(popoverRect, bg, border, radius);

                    LightweaveRoot.PaintSubtree(content, popoverRect);

                    GUI.color = savedColor;

                    Event e = Event.current;
                    if (e.rawType == EventType.MouseDown &&
                        !popoverRect.Contains(e.mousePosition) &&
                        !anchorHere.Contains(e.mousePosition)) {
                        onDismiss?.Invoke();
                        if (e.type == EventType.MouseDown) {
                            e.Use();
                        }
                    }
                }
            );
        };
        return node;
    }
}