using System;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Lightweave.Runtime;

public abstract class LightweaveWindow : Verse.Window {
    [Flags]
    private enum ResizeEdge {
        None = 0,
        Top = 1,
        Bottom = 2,
        Left = 4,
        Right = 8,
    }

    private bool drawOwnCloseX;
    private ResizeEdge activeResize;
    private Vector2 resizeAnchorScreen;
    private Rect resizeStartRect;
    private Texture2D? currentCursor;
    private bool wasMouseDown;
    private bool activeDrag;
    private Vector2 dragAnchorScreen;
    private Rect dragStartRect;

    protected LightweaveWindow() {
        doWindowBackground = false;
        drawShadow = true;
        resizeable = false;
    }

    protected Guid RootId { get; } = Guid.NewGuid();

    protected virtual Theme.Theme? ThemeOverride => null;

    protected virtual Direction? DirectionOverride => null;

    protected virtual bool DrawBorder => true;

    protected virtual Rem BorderRadius => new Rem(0.75f);

    protected virtual Rem BorderThickness => new Rem(1f / 16f);

    protected virtual EdgeInsets BorderPadding => EdgeInsets.All(new Rem(0f));

    protected override float Margin => 0f;

    protected virtual bool EdgeResizable => true;

    protected virtual float EdgeResizeThickness => 8f;

    protected virtual Vector2 MinWindowSize => new Vector2(360f, 240f);

    protected virtual CloseButtonVariant CloseButtonStyle => CloseButtonVariant.Default;

    protected abstract LightweaveNode Build();

    protected virtual Rect? DragRegion(Rect inRect) {
        return null;
    }

    public override void PreOpen() {
        base.PreOpen();
        drawOwnCloseX = doCloseX;
        doCloseX = false;
    }

    public override void DoWindowContents(Rect inRect) {
        if (EdgeResizable) {
            HandleEdgeResize(inRect);
            UpdateEdgeAbsorb(inRect);
        }

        Func<LightweaveNode> rootBuilder = DrawBorder ? BuildBorder : (Func<LightweaveNode>)Build;
        Action? afterContent = drawOwnCloseX ? () => DrawCloseX(inRect) : null;
        LightweaveRoot.Render(inRect, RootId, rootBuilder, DirectionOverride, ThemeOverride, afterContent);

        UpdateCursor(inRect);

        HandleWindowDrag(inRect);
    }

    private void HandleWindowDrag(Rect inRect) {
        if (activeResize != ResizeEdge.None) {
            activeDrag = false;
            return;
        }

        Rect? dragRectOpt = DragRegion(inRect);
        if (!dragRectOpt.HasValue) {
            activeDrag = false;
            return;
        }

        Rect dragRect = dragRectOpt.Value;
        Event e = Event.current;
        bool mouseDownNow = UnityEngine.Input.GetMouseButton(0);

        Vector2 screenTL = new Vector2(
            UnityEngine.Input.mousePosition.x,
            Verse.UI.screenHeight - UnityEngine.Input.mousePosition.y
        );

        if (!activeDrag) {
            if (e.type == EventType.MouseDown
                && e.button == 0
                && dragRect.Contains(e.mousePosition)
                && !LightweaveHitTracker.IsOver(e.mousePosition)) {
                activeDrag = true;
                dragAnchorScreen = screenTL;
                dragStartRect = windowRect;
                e.Use();
            }

            return;
        }

        if (!mouseDownNow) {
            activeDrag = false;
            if (e.type == EventType.MouseUp) {
                e.Use();
            }

            return;
        }

        Vector2 delta = screenTL - dragAnchorScreen;
        Rect next = dragStartRect;
        next.x = Mathf.Clamp(dragStartRect.x + delta.x, 0f, Verse.UI.screenWidth - dragStartRect.width);
        next.y = Mathf.Clamp(dragStartRect.y + delta.y, 0f, Verse.UI.screenHeight - dragStartRect.height);
        windowRect = next;

        if (e.type == EventType.MouseDrag) {
            e.Use();
        }
    }

    /// Dynamically toggle <see cref="Verse.Window.absorbInputAroundWindow"/> so that a click
    /// landing in the edge-buffer zone just outside the window's rect (where we still show a
    /// resize cursor) cannot leak through to the map or a window beneath us. Also held true
    /// for the duration of an active resize so stray clicks during the drag are absorbed.
    private void UpdateEdgeAbsorb(Rect inRect) {
        if (activeResize != ResizeEdge.None) {
            absorbInputAroundWindow = true;
            return;
        }

        Vector2 screenTL = new Vector2(
            UnityEngine.Input.mousePosition.x,
            Verse.UI.screenHeight - UnityEngine.Input.mousePosition.y
        );
        if (windowRect.Contains(screenTL)) {
            absorbInputAroundWindow = false;
            return;
        }

        Vector2 windowLocal = screenTL - new Vector2(windowRect.x, windowRect.y);
        absorbInputAroundWindow = DetectEdge(inRect, windowLocal) != ResizeEdge.None;
    }

    private void HandleEdgeResize(Rect inRect) {
        Event e = Event.current;
        bool mouseDownNow = UnityEngine.Input.GetMouseButton(0);

        Vector2 screenTL = new Vector2(
            UnityEngine.Input.mousePosition.x,
            Verse.UI.screenHeight - UnityEngine.Input.mousePosition.y
        );

        if (activeResize == ResizeEdge.None) {
            // Track MouseDown via Input state transition so presses in the edge buffer
            // zone outside the window's rect still initiate a resize.
            if (mouseDownNow && !wasMouseDown) {
                Vector2 windowLocal = screenTL - new Vector2(windowRect.x, windowRect.y);
                ResizeEdge edge = DetectEdge(inRect, windowLocal);
                if (edge != ResizeEdge.None) {
                    activeResize = edge;
                    resizeAnchorScreen = screenTL;
                    resizeStartRect = windowRect;
                    if (e.type == EventType.MouseDown && e.button == 0) {
                        e.Use();
                    }
                }
            }

            wasMouseDown = mouseDownNow;
            return;
        }

        Vector2 delta = screenTL - resizeAnchorScreen;

        Rect next = resizeStartRect;
        if ((activeResize & ResizeEdge.Right) != 0) {
            next.width = Mathf.Max(MinWindowSize.x, resizeStartRect.width + delta.x);
        }

        if ((activeResize & ResizeEdge.Left) != 0) {
            float proposed = resizeStartRect.width - delta.x;
            if (proposed < MinWindowSize.x) {
                proposed = MinWindowSize.x;
                next.x = resizeStartRect.xMax - proposed;
            } else {
                next.x = resizeStartRect.x + delta.x;
            }

            next.width = proposed;
        }

        if ((activeResize & ResizeEdge.Bottom) != 0) {
            next.height = Mathf.Max(MinWindowSize.y, resizeStartRect.height + delta.y);
        }

        if ((activeResize & ResizeEdge.Top) != 0) {
            float proposed = resizeStartRect.height - delta.y;
            if (proposed < MinWindowSize.y) {
                proposed = MinWindowSize.y;
                next.y = resizeStartRect.yMax - proposed;
            } else {
                next.y = resizeStartRect.y + delta.y;
            }

            next.height = proposed;
        }

        next.x = Mathf.Max(0f, next.x);
        next.y = Mathf.Max(0f, next.y);
        next.width = Mathf.Min(Verse.UI.screenWidth - next.x, next.width);
        next.height = Mathf.Min(Verse.UI.screenHeight - next.y, next.height);

        windowRect = next;

        if (!mouseDownNow) {
            activeResize = ResizeEdge.None;
        }

        if (e.type == EventType.MouseDrag || e.type == EventType.MouseUp) {
            e.Use();
        }

        wasMouseDown = mouseDownNow;
    }

    private void UpdateCursor(Rect inRect) {
        if (activeResize != ResizeEdge.None) {
            ApplyCursor(ResizeCursorFor(activeResize));
            return;
        }

        Vector2 mouse = Event.current.mousePosition;

        ResizeEdge edge = EdgeResizable ? DetectEdge(inRect, mouse) : ResizeEdge.None;
        if (edge != ResizeEdge.None) {
            ApplyCursor(ResizeCursorFor(edge));
            return;
        }

        Rect? dragRect = DragRegion(inRect);
        if (dragRect.HasValue && dragRect.Value.Contains(mouse) && !LightweaveHitTracker.IsOver(mouse)) {
            ApplyCursor(LightweaveCursors.Move);
            return;
        }

        ApplyCursor(null);
    }

    private ResizeEdge DetectEdge(Rect inRect, Vector2 mouse) {
        float t = EdgeResizeThickness;
        bool inside =
            mouse.x >= inRect.x - t &&
            mouse.x <= inRect.xMax + t &&
            mouse.y >= inRect.y - t &&
            mouse.y <= inRect.yMax + t;
        if (!inside) {
            return ResizeEdge.None;
        }

        bool left = mouse.x >= inRect.x - t && mouse.x < inRect.x + t;
        bool right = mouse.x > inRect.xMax - t && mouse.x <= inRect.xMax + t;
        bool top = mouse.y >= inRect.y - t && mouse.y < inRect.y + t;
        bool bottom = mouse.y > inRect.yMax - t && mouse.y <= inRect.yMax + t;

        ResizeEdge edge = ResizeEdge.None;
        if (top) {
            edge |= ResizeEdge.Top;
        }

        if (bottom) {
            edge |= ResizeEdge.Bottom;
        }

        if (left) {
            edge |= ResizeEdge.Left;
        }

        if (right) {
            edge |= ResizeEdge.Right;
        }

        return edge;
    }

    private void ApplyCursor(Texture2D? desired) {
        if (desired == currentCursor) {
            return;
        }

        currentCursor = desired;
        if (desired == null) {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            return;
        }

        Cursor.SetCursor(desired, LightweaveCursors.Hotspot, CursorMode.Auto);
    }

    private static Texture2D? ResizeCursorFor(ResizeEdge edge) {
        if (edge == ResizeEdge.None) {
            return null;
        }

        if (edge == (ResizeEdge.Top | ResizeEdge.Left) ||
            edge == (ResizeEdge.Bottom | ResizeEdge.Right)) {
            return LightweaveCursors.DiagonalNwSe;
        }

        if (edge == (ResizeEdge.Top | ResizeEdge.Right) ||
            edge == (ResizeEdge.Bottom | ResizeEdge.Left)) {
            return LightweaveCursors.DiagonalNeSw;
        }

        if ((edge & (ResizeEdge.Left | ResizeEdge.Right)) != 0) {
            return LightweaveCursors.Horizontal;
        }

        return LightweaveCursors.Vertical;
    }

    private void DrawCloseX(Rect inRect) {
        const float padding = 12f;
        const float size = 18f;
        Rect closeRect = new Rect(
            inRect.xMax - size - padding,
            inRect.y + padding,
            size,
            size
        );
        LightweaveHitTracker.Track(closeRect);

        Theme.Theme theme = RenderContext.Current.Theme;
        Color accent = theme.GetColor(ThemeSlot.SurfaceAccent);
        accent.a = 1f;

        Color baseColor;
        Color hoverColor;
        switch (CloseButtonStyle) {
            case CloseButtonVariant.Primary:
                baseColor = accent;
                hoverColor = Color.Lerp(accent, Color.white, 0.2f);
                break;

            case CloseButtonVariant.Black:
                baseColor = Color.black;
                hoverColor = accent;
                break;

            default:
                // Auto: pick black on light surfaces, white on dark; hover is always the theme accent.
                baseColor = IsLightSurface(theme) ? Color.black : Color.white;
                hoverColor = accent;
                break;
        }

        if (Widgets.ButtonImage(closeRect, TexButton.CloseXSmall, baseColor, hoverColor, true, null)) {
            Close();
        }

        MouseoverSounds.DoRegion(closeRect);
    }

    private static bool IsLightSurface(Theme.Theme theme) {
        Color surface = theme.GetColor(ThemeSlot.SurfacePrimary);
        float luma = 0.299f * surface.r + 0.587f * surface.g + 0.114f * surface.b;
        return luma > 0.5f;
    }

    public override void PostClose() {
        if (currentCursor != null) {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            currentCursor = null;
        }

        LightweaveRoot.Release(RootId);
        base.PostClose();
    }

    private LightweaveNode BuildBorder() {
        EdgeInsets basePad = BorderPadding;
        Rem zero = new Rem(0f);
        EdgeInsets pad = new EdgeInsets(
            Top: (basePad.Top ?? zero) + BorderThickness,
            Bottom: (basePad.Bottom ?? zero) + BorderThickness,
            Left: (basePad.Left ?? zero) + BorderThickness,
            Right: (basePad.Right ?? zero) + BorderThickness
        );
        return Layout.Layout.Box.Create(
            pad,
            new BackgroundSpec.Solid(ThemeSlot.SurfacePrimary),
            BorderSpec.All(BorderThickness, ThemeSlot.BorderSubtle),
            RadiusSpec.All(BorderRadius),
            c => c.Add(Build())
        );
    }
}
