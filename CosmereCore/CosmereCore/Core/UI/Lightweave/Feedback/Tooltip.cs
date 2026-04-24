using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Lightweave.Feedback;

public static class Tooltip {
    private const float HoverDelaySeconds = 0.5f;

    public static LightweaveNode Wrap(
        LightweaveNode children,
        string text,
        Rem? maxWidth = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode content = Typography.Typography.Text(
            text,
            FontRole.Body,
            new Rem(0.875f),
            ThemeSlot.TextPrimary,
            wrap: true
        );
        Vector2? autoSize = MeasureText(text, maxWidth);
        return Wrap(children, content, autoSize, line, file);
    }

    private static Vector2? MeasureText(string text, Rem? maxWidth) {
        if (string.IsNullOrEmpty(text)) {
            return null;
        }

        float pad = new Rem(0.5f).ToPixels() * 2f;
        float maxInner = (maxWidth?.ToPixels() ?? new Rem(20f).ToPixels()) - pad;
        GameFont saved = Text.Font;
        Text.Font = GameFont.Small;
        float h = Text.CalcHeight(text, maxInner);
        Vector2 size = Text.CalcSize(text);
        Text.Font = saved;
        float width = Mathf.Min(size.x, maxInner) + pad;
        float height = h + pad;
        return new Vector2(width, height);
    }

    public static LightweaveNode Wrap(
        LightweaveNode children,
        LightweaveNode content,
        Vector2? preferredSize = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("Tooltip", line, file);
        node.Children.Add(children);

        Hooks.Hooks.RefHandle<float> hoverTimer = Hooks.Hooks.UseRef(0f, line, file);

        node.Paint = (rect, _) => {
            children.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(children, rect);

            bool hovered = Mouse.IsOver(rect);
            Event e = Event.current;

            if (!hovered || e.type == EventType.MouseDown) {
                hoverTimer.Current = 0f;
                return;
            }

            if (e.type == EventType.Repaint) {
                hoverTimer.Current += Time.unscaledDeltaTime;
            }

            if (hoverTimer.Current < HoverDelaySeconds) {
                return;
            }

            Vector2 size = preferredSize ?? new Vector2(new Rem(12f).ToPixels(), new Rem(1.75f).ToPixels());
            Vector2 mouseScreen = GUIUtility.GUIToScreenPoint(e.mousePosition);
            float offsetX = 12f;
            float offsetY = 18f;
            float x = mouseScreen.x + offsetX;
            float y = mouseScreen.y + offsetY;

            if (x + size.x > Screen.width) {
                x = mouseScreen.x - offsetX - size.x;
            }

            if (y + size.y > Screen.height) {
                y = mouseScreen.y - offsetY - size.y;
            }

            if (x < 0f) {
                x = 0f;
            }

            if (y < 0f) {
                y = 0f;
            }

            Rect tooltipScreenRect = new Rect(x, y, size.x, size.y);

            RenderContext.Current.PendingOverlays.Enqueue(() => {
                    Vector2 local = GUIUtility.ScreenToGUIPoint(new Vector2(tooltipScreenRect.x, tooltipScreenRect.y));
                    Rect tooltipRect = new Rect(local.x, local.y, tooltipScreenRect.width, tooltipScreenRect.height);

                    Rect shadowRect = new Rect(
                        tooltipRect.x + 2f,
                        tooltipRect.y + 3f,
                        tooltipRect.width,
                        tooltipRect.height
                    );
                    BackgroundSpec shadowBg = new BackgroundSpec.Solid(new Color(0f, 0f, 0f, 0.35f));
                    PaintBox.Draw(shadowRect, shadowBg, null, null);

                    BackgroundSpec bg = new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised);
                    BorderSpec border = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault);
                    RadiusSpec radius = RadiusSpec.All(new Rem(0.25f));
                    PaintBox.Draw(tooltipRect, bg, border, radius);

                    float pad = new Rem(0.5f).ToPixels();
                    Rect innerRect = new Rect(
                        tooltipRect.x + pad,
                        tooltipRect.y + pad,
                        tooltipRect.width - pad * 2f,
                        tooltipRect.height - pad * 2f
                    );

                    LightweaveRoot.PaintSubtree(content, innerRect);
                }
            );
        };

        return node;
    }
}