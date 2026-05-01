using System;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Runtime;

public sealed class LightweaveScrollStatus {
    public float Height;
    public Vector2 Position;
    public bool VerticalVisible;
    public bool Dragging;
    public float DragAnchor;
    public int ArrowHeld;
    public float NextArrowRepeatAt;
}

public readonly record struct LightweaveScrollView : IDisposable {
    private const float ScrollbarWidth = 10f;
    private const float ScrollbarRightGap = 0f;
    private const float MinThumbHeight = 24f;
    private const float ArrowSize = 18f;
    private const float BarGapFromArrow = 2f;
    private const float ThumbHorizontalPadding = 2f;
    private const float ArrowStepPixels = 40f;
    private const float ArrowRepeatInitialDelay = 0.35f;
    private const float ArrowRepeatInterval = 0.04f;

    private static float ScrollbarLeftGap => new Rem(1f).ToPixels();
    private static float ArrowOuterPadding => new Rem(0.5f).ToPixels();
    private static float ArrowInnerPadding => new Rem(0.25f).ToPixels();
    private const float ArrowSidePadding = 1f;

    public static float GutterPixels(bool verticalVisible) {
        return verticalVisible ? ScrollbarLeftGap + ScrollbarWidth + ScrollbarRightGap : 0f;
    }

    private readonly Rect outRect;
    private readonly float viewHeight;
    private readonly float contentHeight;
    private readonly LightweaveScrollStatus status;
    private readonly bool showScrollbar;

    public readonly Rect rect;

    public LightweaveScrollView(
        Rect outRect,
        LightweaveScrollStatus status,
        bool showScrollbar = true
    ) {
        this.outRect = outRect;
        this.status = status;
        this.showScrollbar = showScrollbar;

        float declared = Math.Max(status.Height, outRect.height);
        bool overflows = declared - 0.1f >= outRect.height;
        status.VerticalVisible = overflows && showScrollbar;

        float gutter = GutterPixels(status.VerticalVisible);
        contentHeight = declared;
        viewHeight = outRect.height;
        rect = new Rect(0f, 0f, outRect.width - gutter, declared);

        status.Height = 0f;
        Widgets.BeginScrollView(outRect, ref status.Position, rect, false);
    }

    public ref float Height => ref status.Height;

    public bool CanCull(float entryHeight, float entryY) {
        return entryY + entryHeight < status.Position.y ||
               entryY > status.Position.y + viewHeight;
    }

    public void Dispose() {
        Widgets.EndScrollView();

        if (!status.VerticalVisible) {
            return;
        }

        PaintScrollbar();
    }

    private void PaintScrollbar() {
        Theme.Theme theme = RenderContext.Current.Theme;
        Event e = Event.current;

        float trackX = outRect.xMax - ScrollbarWidth - ScrollbarRightGap;
        Rect upArrow = new Rect(trackX, outRect.y, ScrollbarWidth, ArrowSize);
        Rect downArrow = new Rect(trackX, outRect.yMax - ArrowSize, ScrollbarWidth, ArrowSize);

        float trackY = upArrow.yMax + BarGapFromArrow;
        float trackHeight = Mathf.Max(0f, downArrow.y - BarGapFromArrow - trackY);
        Rect trackRect = new Rect(trackX, trackY, ScrollbarWidth, trackHeight);

        float scrollRange = Mathf.Max(0f, contentHeight - viewHeight);
        float thumbHeight = trackRect.height > 0f
            ? Mathf.Max(MinThumbHeight, trackRect.height * Mathf.Clamp01(viewHeight / contentHeight))
            : 0f;
        thumbHeight = Mathf.Min(thumbHeight, trackRect.height);
        float trackRange = Mathf.Max(0f, trackRect.height - thumbHeight);
        float scrollNorm = scrollRange > 0f ? Mathf.Clamp01(status.Position.y / scrollRange) : 0f;
        Rect thumbRect = new Rect(
            trackRect.x + ThumbHorizontalPadding,
            trackRect.y + trackRange * scrollNorm,
            trackRect.width - ThumbHorizontalPadding * 2f,
            thumbHeight
        );

        bool hovering = thumbRect.Contains(e.mousePosition);
        bool active = status.Dragging;

        Color saved = GUI.color;
        float pillRadius = ScrollbarWidth * 0.5f;

        // Unified pill column behind arrows + track so everything reads as one surface.
        Rect columnRect = new Rect(trackX, outRect.y, ScrollbarWidth, outRect.height);
        Color bgColor = theme.GetColor(ThemeSlot.SurfaceSunken);
        bgColor.a = 0.35f;
        GUI.DrawTexture(
            columnRect,
            Texture2D.whiteTexture,
            ScaleMode.StretchToFill,
            true,
            0f,
            bgColor,
            Vector4.zero,
            new Vector4(pillRadius, pillRadius, pillRadius, pillRadius)
        );

        ThemeSlot thumbSlot = active
            ? ThemeSlot.BorderFocus
            : hovering
                ? ThemeSlot.BorderHover
                : ThemeSlot.BorderDefault;
        Color thumbColor = theme.GetColor(thumbSlot);
        if (!active && !hovering) {
            thumbColor.a *= 0.85f;
        }

        GUI.DrawTexture(
            thumbRect,
            Texture2D.whiteTexture,
            ScaleMode.StretchToFill,
            true,
            0f,
            thumbColor,
            Vector4.zero,
            new Vector4(pillRadius, pillRadius, pillRadius, pillRadius)
        );

        PaintArrow(upArrow, true, theme, e);
        PaintArrow(downArrow, false, theme, e);

        GUI.color = saved;

        HandleScrollbarEvents(e, trackRect, thumbRect, upArrow, downArrow, scrollRange, trackRange);
        HandleArrowHold(scrollRange, upArrow, downArrow);
    }

    private void PaintArrow(Rect rect, bool up, Theme.Theme theme, Event e) {
        bool hovering = rect.Contains(e.mousePosition);
        bool held = status.ArrowHeld == (up ? 1 : 2);

        ThemeSlot slot = held
            ? ThemeSlot.BorderFocus
            : hovering
                ? ThemeSlot.BorderHover
                : ThemeSlot.BorderDefault;
        Color fill = theme.GetColor(slot);
        if (!held && !hovering) {
            fill.a *= 0.85f;
        }

        // Outer padding pushes the triangle AWAY from the scrollbar's outer edge
        // (top for the up arrow, bottom for the down arrow). Inner padding keeps
        // the base from touching the track. Triangle ends up wider than tall.
        float outerPad = ArrowOuterPadding;
        float innerPad = ArrowInnerPadding;
        Rect triRect = up
            ? new Rect(
                rect.x + ArrowSidePadding,
                rect.y + outerPad,
                rect.width - ArrowSidePadding * 2f,
                rect.height - outerPad - innerPad
            )
            : new Rect(
                rect.x + ArrowSidePadding,
                rect.y + innerPad,
                rect.width - ArrowSidePadding * 2f,
                rect.height - innerPad - outerPad
            );

        if (triRect.width <= 0f || triRect.height <= 0f) {
            return;
        }

        GUI.DrawTexture(
            triRect,
            GetArrowTexture(up),
            ScaleMode.StretchToFill,
            true,
            0f,
            fill,
            Vector4.zero,
            Vector4.zero
        );
    }

    private static Texture2D GetArrowTexture(bool up) {
        if (up) {
            return LightweaveScrollArrows.UpArrowTex ??= BuildArrowTexture(true);
        }

        return LightweaveScrollArrows.DownArrowTex ??= BuildArrowTexture(false);
    }

    /// Generates a 128x128 RGBA texture containing an antialiased filled triangle
    /// (apex at GUI-top for up-arrows, apex at GUI-bottom for down-arrows). The
    /// texture is tinted via GUI.DrawTexture's color parameter at draw time.
    private static Texture2D BuildArrowTexture(bool up) {
        const int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color32[] pixels = new Color32[size * size];
        for (int py = 0; py < size; py++) {
            int guiRow = size - 1 - py;
            float t = guiRow / (float)(size - 1);
            float apexFrac = up ? t : 1f - t;
            float halfWidth = apexFrac * 0.5f;
            for (int px = 0; px < size; px++) {
                float normX = (px + 0.5f) / size;
                float dist = Mathf.Abs(normX - 0.5f);
                float edge = halfWidth - dist;
                float alpha = Mathf.Clamp01(edge * size);
                byte a = (byte)Mathf.RoundToInt(alpha * 255f);
                pixels[py * size + px] = new Color32(255, 255, 255, a);
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply(false, false);
        return tex;
    }

    private void HandleScrollbarEvents(
        Event e,
        Rect trackRect,
        Rect thumbRect,
        Rect upArrow,
        Rect downArrow,
        float scrollRange,
        float trackRange
    ) {
        if (e.type == EventType.MouseDown && e.button == 0) {
            if (upArrow.Contains(e.mousePosition)) {
                ScrollBy(-ArrowStepPixels, scrollRange);
                status.ArrowHeld = 1;
                status.NextArrowRepeatAt = Time.realtimeSinceStartup + ArrowRepeatInitialDelay;
                e.Use();
                return;
            }

            if (downArrow.Contains(e.mousePosition)) {
                ScrollBy(ArrowStepPixels, scrollRange);
                status.ArrowHeld = 2;
                status.NextArrowRepeatAt = Time.realtimeSinceStartup + ArrowRepeatInitialDelay;
                e.Use();
                return;
            }

            if (thumbRect.Contains(e.mousePosition)) {
                status.Dragging = true;
                status.DragAnchor = e.mousePosition.y - thumbRect.y;
                e.Use();
                return;
            }

            if (trackRect.Contains(e.mousePosition)) {
                bool above = e.mousePosition.y < thumbRect.y;
                float delta = viewHeight * 0.9f * (above ? -1f : 1f);
                ScrollBy(delta, scrollRange);
                e.Use();
                return;
            }
        }

        if (status.Dragging) {
            if (e.type == EventType.MouseDrag) {
                float desiredThumbY = e.mousePosition.y - status.DragAnchor;
                float clampedY = Mathf.Clamp(desiredThumbY, trackRect.y, trackRect.y + trackRange);
                float norm = trackRange > 0f ? (clampedY - trackRect.y) / trackRange : 0f;
                status.Position = new Vector2(status.Position.x, norm * scrollRange);
                e.Use();
                return;
            }

            if (e.type == EventType.MouseUp) {
                status.Dragging = false;
                e.Use();
                return;
            }
        }

        if (status.ArrowHeld != 0 && e.type == EventType.MouseUp) {
            status.ArrowHeld = 0;
            e.Use();
        }
    }

    private void HandleArrowHold(float scrollRange, Rect upArrow, Rect downArrow) {
        if (status.ArrowHeld == 0) {
            return;
        }

        if (!UnityEngine.Input.GetMouseButton(0)) {
            status.ArrowHeld = 0;
            return;
        }

        Vector2 mouse = Event.current.mousePosition;
        bool stillInButton = status.ArrowHeld == 1
            ? upArrow.Contains(mouse)
            : downArrow.Contains(mouse);
        if (!stillInButton) {
            return;
        }

        float now = Time.realtimeSinceStartup;
        if (now < status.NextArrowRepeatAt) {
            return;
        }

        float delta = status.ArrowHeld == 1 ? -ArrowStepPixels : ArrowStepPixels;
        ScrollBy(delta, scrollRange);
        status.NextArrowRepeatAt = now + ArrowRepeatInterval;
    }

    private void ScrollBy(float delta, float scrollRange) {
        float next = Mathf.Clamp(status.Position.y + delta, 0f, scrollRange);
        status.Position = new Vector2(status.Position.x, next);
    }
}

internal static class LightweaveScrollArrows {
    internal static Texture2D? UpArrowTex;
    internal static Texture2D? DownArrowTex;
}
