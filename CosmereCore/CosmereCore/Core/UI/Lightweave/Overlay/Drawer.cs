using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Hooks;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Overlay;

public enum DrawerSide
{
    Left,
    Right,
    Top,
    Bottom,
}

public static class Drawer
{
    private static readonly Func<float, float> EaseOutCubic = t => 1f - Mathf.Pow(1f - t, 3f);

    public static LightweaveNode Create(
        bool isOpen,
        DrawerSide side,
        Func<LightweaveNode> content,
        Action onDismiss,
        Rem? size = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        LightweaveNode node = NodeBuilder.New($"Drawer:{side}", line, file);
        node.Paint = (_, _) =>
        {
            float target = isOpen ? 1f : 0f;
            float progress = UseAnim.Animate(target, 0.22f, EaseOutCubic, line, file);

            if (!isOpen && progress <= 0.001f)
            {
                return;
            }

            float screenW = Screen.width;
            float screenH = Screen.height;

            float widthPx;
            float heightPx;
            if (side == DrawerSide.Left || side == DrawerSide.Right)
            {
                widthPx = (size ?? new Rem(20f)).ToPixels();
                heightPx = screenH;
            }
            else
            {
                widthPx = screenW;
                heightPx = (size ?? new Rem(12f)).ToPixels();
            }

            float restX;
            float restY;
            float offscreenX;
            float offscreenY;
            switch (side)
            {
                case DrawerSide.Left:
                    restX = 0f;
                    restY = 0f;
                    offscreenX = -widthPx;
                    offscreenY = 0f;
                    break;
                case DrawerSide.Right:
                    restX = screenW - widthPx;
                    restY = 0f;
                    offscreenX = screenW;
                    offscreenY = 0f;
                    break;
                case DrawerSide.Top:
                    restX = 0f;
                    restY = 0f;
                    offscreenX = 0f;
                    offscreenY = -heightPx;
                    break;
                default:
                    restX = 0f;
                    restY = screenH - heightPx;
                    offscreenX = 0f;
                    offscreenY = screenH;
                    break;
            }

            float drawerX = Mathf.Lerp(offscreenX, restX, progress);
            float drawerY = Mathf.Lerp(offscreenY, restY, progress);
            Rect drawerRect = new Rect(drawerX, drawerY, widthPx, heightPx);
            float scrimAlpha = progress * 0.35f;

            RenderContext.Current.PendingOverlays.Enqueue(() =>
            {
                Rect screenRect = new Rect(0f, 0f, screenW, screenH);
                BackgroundSpec scrimBg = new BackgroundSpec.Solid(new Color(0f, 0f, 0f, scrimAlpha));
                PaintBox.Draw(screenRect, scrimBg, null, null);

                BackgroundSpec drawerBg = new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised);
                BorderSpec? drawerBorder = ResolveBorder(side);
                PaintBox.Draw(drawerRect, drawerBg, drawerBorder, null);

                float padPx = SpacingScale.Md.ToPixels();
                Rect innerRect = new Rect(
                    drawerRect.x + padPx,
                    drawerRect.y + padPx,
                    Mathf.Max(0f, drawerRect.width - padPx * 2f),
                    Mathf.Max(0f, drawerRect.height - padPx * 2f));

                LightweaveNode inner = content();
                LightweaveRoot.PaintSubtree(inner, innerRect);

                Event e = Event.current;
                if (e.type == EventType.MouseDown
                    && !drawerRect.Contains(e.mousePosition)
                    && screenRect.Contains(e.mousePosition))
                {
                    onDismiss?.Invoke();
                    e.Use();
                }
                else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
                {
                    onDismiss?.Invoke();
                    e.Use();
                }
            });
        };
        return node;
    }

    private static BorderSpec ResolveBorder(DrawerSide side)
    {
        Rem thickness = new Rem(1f / 16f);
        switch (side)
        {
            case DrawerSide.Left:
                return new BorderSpec(Right: thickness, Color: ThemeSlot.BorderDefault);
            case DrawerSide.Right:
                return new BorderSpec(Left: thickness, Color: ThemeSlot.BorderDefault);
            case DrawerSide.Top:
                return new BorderSpec(Bottom: thickness, Color: ThemeSlot.BorderDefault);
            default:
                return new BorderSpec(Top: thickness, Color: ThemeSlot.BorderDefault);
        }
    }
}
