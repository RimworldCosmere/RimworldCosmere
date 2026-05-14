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
    public float LastContentHeight;
    public float LastViewportHeight;
    public float LastScrollAtRealtime;
    public float LastMouseEnterAtRealtime;
    public bool MouseInsideViewport;
    public float LastObservedPositionY;
}

public readonly record struct LightweaveScrollView : IDisposable {
    private const float StandardWidth = 10f;
    private const float SlimWidth = 6f;
    private const float MinimalWidth = 4f;

    private const float ScrollbarRightGap = 0f;
    private const float MinThumbHeight = 24f;
    private const float ArrowSize = 18f;
    private const float BarGapFromArrow = 2f;
    private const float ThumbHorizontalPadding = 2f;
    private const float ArrowStepPixels = 40f;
    private const float ArrowRepeatInitialDelay = 0.35f;
    private const float ArrowRepeatInterval = 0.04f;

    private const float RevealAfterScrollDuration = 0.5f;
    private const float RevealAfterMouseEnterDuration = 0.8f;
    private const float RevealFadeOutDuration = 0.2f;

    private static float ScrollbarLeftGap => new Rem(1f).ToPixels();
    private static float ArrowOuterPadding => new Rem(0.5f).ToPixels();
    private static float ArrowInnerPadding => new Rem(0.25f).ToPixels();
    private const float ArrowSidePadding = 1f;

    public static float WidthFor(ScrollbarStyle style) {
        switch (style) {
            case ScrollbarStyle.Slim:
                return SlimWidth;
            case ScrollbarStyle.Minimal:
                return MinimalWidth;
            case ScrollbarStyle.Standard:
            default:
                return StandardWidth;
        }
    }


    private static float ScrollbarRightInsetFor(ScrollbarMode mode) {
        return mode == ScrollbarMode.Auto ? new Rem(0.375f).ToPixels() : ScrollbarRightGap;
    }

    public static float GutterPixels(bool verticalVisible, ScrollbarMode mode, ScrollbarStyle style) {
        if (!verticalVisible || mode == ScrollbarMode.Never) {
            return 0f;
        }

        if (mode == ScrollbarMode.Auto) {
            return 0f;
        }

        return ScrollbarLeftGap + WidthFor(style) + ScrollbarRightGap;
    }

    public static float GutterPixels(bool verticalVisible) {
        return GutterPixels(verticalVisible, ScrollbarMode.Always, ScrollbarStyle.Standard);
    }

    private readonly Rect outRect;
    private readonly float viewHeight;
    private readonly float contentHeight;
    private readonly LightweaveScrollStatus status;
    private readonly ScrollbarMode mode;
    private readonly ScrollbarStyle style;

    public readonly Rect rect;

    public LightweaveScrollView(
        Rect outRect,
        LightweaveScrollStatus status,
        ScrollbarMode mode = ScrollbarMode.Auto,
        ScrollbarStyle style = ScrollbarStyle.Slim
    ) {
        this.outRect = outRect;
        this.status = status;
        this.mode = mode;
        this.style = style;

        float declared = Math.Max(status.Height, outRect.height);
        bool overflows = declared - 0.1f >= outRect.height;
        status.VerticalVisible = overflows && mode != ScrollbarMode.Never;

        float gutter = GutterPixels(status.VerticalVisible, mode, style);
        contentHeight = declared;
        viewHeight = outRect.height;
        rect = new Rect(0f, 0f, outRect.width - gutter, declared);

        status.LastContentHeight = declared;
        status.LastViewportHeight = outRect.height;
        status.Height = 0f;

        float prevY = status.Position.y;
        Widgets.BeginScrollView(outRect, ref status.Position, rect, false);
        if (Math.Abs(status.Position.y - prevY) > 0.01f) {
            status.LastScrollAtRealtime = Time.realtimeSinceStartup;
        }
    }

    public LightweaveScrollView(Rect outRect, LightweaveScrollStatus status, bool showScrollbar)
        : this(outRect, status, showScrollbar ? ScrollbarMode.Auto : ScrollbarMode.Never, ScrollbarStyle.Slim) {
    }

    public ref float Height => ref status.Height;

    public bool CanCull(float entryHeight, float entryY) {
        return entryY + entryHeight < status.Position.y ||
               entryY > status.Position.y + viewHeight;
    }

    public void Dispose() {
        Widgets.EndScrollView();

        if (!status.VerticalVisible || mode == ScrollbarMode.Never) {
            return;
        }

        UpdateMouseHoverState();
        float alpha = ComputeRevealAlpha();
        if (alpha <= 0.001f) {
            return;
        }

        PaintScrollbar(alpha);
    }

    private void UpdateMouseHoverState() {
        if (Event.current.type != EventType.Repaint) {
            return;
        }

        bool inside = outRect.Contains(Event.current.mousePosition);
        if (inside && !status.MouseInsideViewport) {
            status.LastMouseEnterAtRealtime = Time.realtimeSinceStartup;
        }

        status.MouseInsideViewport = inside;
    }

    private float ComputeRevealAlpha() {
        if (mode == ScrollbarMode.Always) {
            return 1f;
        }

        if (mode == ScrollbarMode.Never) {
            return 0f;
        }

        if (status.Dragging || status.ArrowHeld != 0) {
            return 1f;
        }

        float width = WidthFor(style);
        float rightInset = ScrollbarRightInsetFor(mode);
        Rect scrollbarZone = new Rect(
            outRect.xMax - width - rightInset,
            outRect.y,
            width,
            outRect.height
        );
        if (scrollbarZone.Contains(Event.current.mousePosition)) {
            return 1f;
        }

        float now = Time.realtimeSinceStartup;
        float scrollAge = now - status.LastScrollAtRealtime;
        float hoverAge = now - status.LastMouseEnterAtRealtime;

        float scrollAlpha = AlphaFromAge(scrollAge, RevealAfterScrollDuration);
        float hoverAlpha = AlphaFromAge(hoverAge, RevealAfterMouseEnterDuration);
        return Mathf.Max(scrollAlpha, hoverAlpha);
    }

    private static float AlphaFromAge(float age, float visibleFor) {
        if (age < 0f) {
            return 0f;
        }

        if (age < visibleFor) {
            return 1f;
        }

        if (age < visibleFor + RevealFadeOutDuration) {
            return 1f - (age - visibleFor) / RevealFadeOutDuration;
        }

        return 0f;
    }

    private void PaintScrollbar(float alpha) {
        Theme.Theme theme = RenderContext.Current.Theme;
        Event e = Event.current;

        float width = WidthFor(style);
        bool showArrows = style == ScrollbarStyle.Standard;
        bool showTrackBg = style != ScrollbarStyle.Minimal;

        float rightInset = ScrollbarRightInsetFor(mode);
        float trackX = outRect.xMax - width - rightInset;

        Rect upArrow = new Rect(trackX, outRect.y, width, ArrowSize);
        Rect downArrow = new Rect(trackX, outRect.yMax - ArrowSize, width, ArrowSize);

        float overlayMargin = mode == ScrollbarMode.Auto ? new Rem(0.125f).ToPixels() : 0f;
        float trackY = showArrows ? upArrow.yMax + BarGapFromArrow : outRect.y + overlayMargin;
        float trackBottom = showArrows ? downArrow.y - BarGapFromArrow : outRect.yMax - overlayMargin;
        float trackHeight = Mathf.Max(0f, trackBottom - trackY);
        Rect trackRect = new Rect(trackX, trackY, width, trackHeight);

        float scrollRange = Mathf.Max(0f, contentHeight - viewHeight);
        float thumbHeight = trackRect.height > 0f
            ? Mathf.Max(MinThumbHeight, trackRect.height * Mathf.Clamp01(viewHeight / contentHeight))
            : 0f;
        thumbHeight = Mathf.Min(thumbHeight, trackRect.height);
        float trackRange = Mathf.Max(0f, trackRect.height - thumbHeight);
        float scrollNorm = scrollRange > 0f ? Mathf.Clamp01(status.Position.y / scrollRange) : 0f;
        float thumbInset = style == ScrollbarStyle.Standard ? ThumbHorizontalPadding : 0f;
        Rect thumbRect = new Rect(
            trackRect.x + thumbInset,
            trackRect.y + trackRange * scrollNorm,
            trackRect.width - thumbInset * 2f,
            thumbHeight
        );

        bool hovering = thumbRect.Contains(e.mousePosition);
        bool active = status.Dragging;

        Color saved = GUI.color;
        float pillRadius = width * 0.5f;

        if (showTrackBg) {
            Rect columnRect = showArrows
                ? new Rect(trackX, outRect.y, width, outRect.height)
                : trackRect;
            Color bgColor = theme.GetColor(ThemeSlot.SurfaceSunken);
            bgColor.a *= 0.35f * alpha;
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
        }

        ThemeSlot thumbSlot = active
            ? ThemeSlot.BorderFocus
            : hovering
                ? ThemeSlot.BorderHover
                : ThemeSlot.BorderDefault;
        Color thumbColor = theme.GetColor(thumbSlot);
        if (!active && !hovering) {
            thumbColor.a *= 0.85f;
        }

        thumbColor.a *= alpha;

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

        if (showArrows) {
            PaintArrow(upArrow, true, theme, e, alpha);
            PaintArrow(downArrow, false, theme, e, alpha);
        }

        GUI.color = saved;

        HandleScrollbarEvents(e, trackRect, thumbRect, upArrow, downArrow, scrollRange, trackRange, showArrows);
        if (showArrows) {
            HandleArrowHold(scrollRange, upArrow, downArrow);
        }
    }

    private void PaintArrow(Rect rect, bool up, Theme.Theme theme, Event e, float alpha) {
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

        fill.a *= alpha;

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
        float trackRange,
        bool showArrows
    ) {
        if (e.type == EventType.MouseDown && e.button == 0) {
            if (showArrows && upArrow.Contains(e.mousePosition)) {
                ScrollBy(-ArrowStepPixels, scrollRange);
                status.ArrowHeld = 1;
                status.NextArrowRepeatAt = Time.realtimeSinceStartup + ArrowRepeatInitialDelay;
                status.LastScrollAtRealtime = Time.realtimeSinceStartup;
                e.Use();
                return;
            }

            if (showArrows && downArrow.Contains(e.mousePosition)) {
                ScrollBy(ArrowStepPixels, scrollRange);
                status.ArrowHeld = 2;
                status.NextArrowRepeatAt = Time.realtimeSinceStartup + ArrowRepeatInitialDelay;
                status.LastScrollAtRealtime = Time.realtimeSinceStartup;
                e.Use();
                return;
            }

            if (thumbRect.Contains(e.mousePosition)) {
                status.Dragging = true;
                status.DragAnchor = e.mousePosition.y - thumbRect.y;
                status.LastScrollAtRealtime = Time.realtimeSinceStartup;
                e.Use();
                return;
            }

            if (trackRect.Contains(e.mousePosition)) {
                bool above = e.mousePosition.y < thumbRect.y;
                float delta = viewHeight * 0.9f * (above ? -1f : 1f);
                ScrollBy(delta, scrollRange);
                status.LastScrollAtRealtime = Time.realtimeSinceStartup;
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
                status.LastScrollAtRealtime = Time.realtimeSinceStartup;
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
        status.LastScrollAtRealtime = now;
    }

    private void ScrollBy(float delta, float scrollRange) {
        float next = Mathf.Clamp(status.Position.y + delta, 0f, scrollRange);
        status.Position = new Vector2(status.Position.x, next);
    }
}

[StaticConstructorOnStartup]
internal static class LightweaveScrollArrows {
    internal static Texture2D? UpArrowTex;
    internal static Texture2D? DownArrowTex;
}
