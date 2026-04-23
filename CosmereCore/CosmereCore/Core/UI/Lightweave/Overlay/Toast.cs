using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Hooks;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Overlay;

public enum ToastKind
{
    Info,
    Success,
    Warning,
    Danger,
}

public enum ToastCorner
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
}

public sealed record ToastMessage(
    string Id,
    string Text,
    ToastKind Kind = ToastKind.Info,
    float DurationSeconds = 4f);

public static class Toast
{
    public static LightweaveNode Create(
        IReadOnlyList<ToastMessage> toasts,
        Action<string> onDismiss,
        ToastCorner corner = ToastCorner.BottomRight,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        Hooks.Hooks.RefHandle<Dictionary<string, float>> spawnsRef = Hooks.Hooks.UseRef<Dictionary<string, float>>(
            new Dictionary<string, float>(), line, file);

        LightweaveNode node = NodeBuilder.New($"Toast:{corner}", line, file);
        node.Paint = (_, _) =>
        {
            Dictionary<string, float> spawns = spawnsRef.Current;
            float now = Time.unscaledTime;

            HashSet<string> presentIds = new HashSet<string>();
            for (int i = 0; i < toasts.Count; i++)
            {
                ToastMessage msg = toasts[i];
                presentIds.Add(msg.Id);
                if (!spawns.ContainsKey(msg.Id))
                {
                    spawns[msg.Id] = now;
                }
            }

            List<string> stale = new List<string>();
            foreach (KeyValuePair<string, float> kvp in spawns)
            {
                if (!presentIds.Contains(kvp.Key))
                {
                    stale.Add(kvp.Key);
                }
            }
            for (int i = 0; i < stale.Count; i++)
            {
                spawns.Remove(stale[i]);
            }

            List<string> expired = new List<string>();
            for (int i = 0; i < toasts.Count; i++)
            {
                ToastMessage msg = toasts[i];
                float spawnTime = spawns[msg.Id];
                if (now - spawnTime > msg.DurationSeconds)
                {
                    expired.Add(msg.Id);
                }
            }
            for (int i = 0; i < expired.Count; i++)
            {
                onDismiss?.Invoke(expired[i]);
            }

            if (toasts.Count == 0)
            {
                return;
            }

            float widthPx = new Rem(20f).ToPixels();
            float minHeightPx = new Rem(3.5f).ToPixels();
            float gapPx = SpacingScale.Sm.ToPixels();
            float marginPx = new Rem(1.5f).ToPixels();
            float fadePx = 0.2f;
            float screenW = Screen.width;
            float screenH = Screen.height;

            bool anchorRight = corner == ToastCorner.TopRight || corner == ToastCorner.BottomRight;
            bool anchorBottom = corner == ToastCorner.BottomLeft || corner == ToastCorner.BottomRight;

            float anchorX = anchorRight ? screenW - widthPx - marginPx : marginPx;
            float cursorY = anchorBottom ? screenH - marginPx : marginPx;

            int count = toasts.Count;
            float[] heights = new float[count];
            float[] alphas = new float[count];
            ToastMessage[] snapshot = new ToastMessage[count];

            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            Font font = theme.GetFont(FontRole.Body);
            int textPixelSize = Mathf.RoundToInt(new Rem(0.9375f).ToPixels());
            GUIStyle textStyle = GuiStyleCache.Get(font, textPixelSize, FontStyle.Normal);
            textStyle.alignment = TextAnchor.UpperLeft;
            textStyle.wordWrap = true;

            float stripWidth = 8f;
            float padPx = SpacingScale.Md.ToPixels();
            float closeSize = new Rem(1.25f).ToPixels();

            for (int i = 0; i < count; i++)
            {
                ToastMessage msg = toasts[i];
                snapshot[i] = msg;
                float spawnTime = spawns[msg.Id];
                float age = now - spawnTime;
                float remaining = msg.DurationSeconds - age;
                float fadeIn = fadePx > 0f ? Mathf.Clamp01(age / fadePx) : 1f;
                float fadeOut = fadePx > 0f ? Mathf.Clamp01(remaining / fadePx) : 1f;
                alphas[i] = fadeIn * fadeOut;

                float textLeft;
                float textRight;
                if (dir == Direction.Ltr)
                {
                    textLeft = padPx + stripWidth;
                    textRight = padPx + closeSize + padPx;
                }
                else
                {
                    textLeft = padPx + closeSize + padPx;
                    textRight = padPx + stripWidth;
                }
                float textWidth = Mathf.Max(0f, widthPx - textLeft - textRight);
                float textHeight = textStyle.CalcHeight(new GUIContent(msg.Text), textWidth);
                float rowHeight = Mathf.Max(minHeightPx, textHeight + padPx * 2f);
                heights[i] = rowHeight;

                if (alphas[i] < 1f)
                {
                    AnimationClock.RegisterActive(RenderContext.Current.RootId);
                }
            }

            float[] positionsY = new float[count];
            if (anchorBottom)
            {
                for (int i = count - 1; i >= 0; i--)
                {
                    cursorY -= heights[i];
                    positionsY[i] = cursorY;
                    cursorY -= gapPx;
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    positionsY[i] = cursorY;
                    cursorY += heights[i] + gapPx;
                }
            }

            RenderContext.Current.PendingOverlays.Enqueue(() =>
            {
                Color savedGuiColor = GUI.color;
                for (int i = 0; i < count; i++)
                {
                    ToastMessage msg = snapshot[i];
                    float alpha = alphas[i];
                    Rect toastRect = new Rect(anchorX, positionsY[i], widthPx, heights[i]);

                    Color savedInner = GUI.color;
                    GUI.color = new Color(1f, 1f, 1f, alpha);

                    BackgroundSpec bg = new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised);
                    BorderSpec border = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault);
                    RadiusSpec radius = RadiusSpec.All(new Rem(0.25f));
                    PaintBox.Draw(toastRect, bg, border, radius);

                    ThemeSlot stripSlot = StripSlot(msg.Kind);
                    Color stripColor = theme.GetColor(stripSlot);
                    Color stripWithAlpha = new Color(stripColor.r, stripColor.g, stripColor.b, stripColor.a * alpha);
                    float stripX = dir == Direction.Ltr
                        ? toastRect.x
                        : toastRect.xMax - stripWidth;
                    Rect stripRect = new Rect(stripX, toastRect.y, stripWidth, toastRect.height);
                    GUI.color = new Color(1f, 1f, 1f, 1f);
                    PaintBox.Draw(stripRect, new BackgroundSpec.Solid(stripWithAlpha), null, null);
                    GUI.color = new Color(1f, 1f, 1f, alpha);

                    float textLeft;
                    float textRight;
                    if (dir == Direction.Ltr)
                    {
                        textLeft = padPx + stripWidth;
                        textRight = padPx + closeSize + padPx;
                    }
                    else
                    {
                        textLeft = padPx + closeSize + padPx;
                        textRight = padPx + stripWidth;
                    }

                    Rect textRect = new Rect(
                        toastRect.x + textLeft,
                        toastRect.y + padPx,
                        Mathf.Max(0f, toastRect.width - textLeft - textRight),
                        Mathf.Max(0f, toastRect.height - padPx * 2f));

                    Color savedTextColor = GUI.color;
                    Color textColor = theme.GetColor(ThemeSlot.TextPrimary);
                    GUI.color = new Color(textColor.r, textColor.g, textColor.b, textColor.a * alpha);
                    GUI.Label(RectSnap.Snap(textRect), msg.Text, textStyle);
                    GUI.color = savedTextColor;

                    float closeX = dir == Direction.Ltr
                        ? toastRect.xMax - padPx - closeSize
                        : toastRect.x + padPx;
                    Rect closeRect = new Rect(
                        closeX,
                        toastRect.y + padPx,
                        closeSize,
                        closeSize);

                    Font closeFont = theme.GetFont(FontRole.BodyBold);
                    int closePixelSize = Mathf.RoundToInt(new Rem(1f).ToPixels());
                    GUIStyle closeStyle = GuiStyleCache.Get(closeFont, closePixelSize, FontStyle.Bold);
                    closeStyle.alignment = TextAnchor.MiddleCenter;

                    Color savedCloseColor = GUI.color;
                    Color closeColor = theme.GetColor(ThemeSlot.TextMuted);
                    GUI.color = new Color(closeColor.r, closeColor.g, closeColor.b, closeColor.a * alpha);
                    GUI.Label(RectSnap.Snap(closeRect), "×", closeStyle);
                    GUI.color = savedCloseColor;

                    GUI.color = savedInner;

                    Event e = Event.current;
                    if (e.type == EventType.MouseUp
                        && e.button == 0
                        && closeRect.Contains(e.mousePosition))
                    {
                        onDismiss?.Invoke(msg.Id);
                        e.Use();
                    }
                }
                GUI.color = savedGuiColor;
            });
        };
        return node;
    }

    private static ThemeSlot StripSlot(ToastKind kind)
    {
        switch (kind)
        {
            case ToastKind.Success:
                return ThemeSlot.StatusSuccess;
            case ToastKind.Warning:
                return ThemeSlot.StatusWarning;
            case ToastKind.Danger:
                return ThemeSlot.StatusDanger;
            default:
                return ThemeSlot.SurfaceAccent;
        }
    }
}
