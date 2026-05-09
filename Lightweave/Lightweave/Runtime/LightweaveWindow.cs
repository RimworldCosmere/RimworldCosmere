using System;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Theme;
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
    private float lastDragClickTime = -1f;
    private Vector2 lastDragClickPos;
    private bool isMaximized;
    private Rect prerestoreRect;
    private bool positionRestored;

    protected LightweaveWindow() {
        doWindowBackground = false;
        drawShadow = true;
        resizeable = false;
        closeOnCancel = true;
        draggable = true;
    }

    protected Guid RootId { get; } = Guid.NewGuid();

    protected virtual Theme.Theme? ThemeOverride => null;

    protected virtual Direction? DirectionOverride => null;

    [DocOverride("Draw the rounded border + surface fill around the window content.", TypeOverride = "bool", DefaultOverride = "true")]
    protected virtual bool DrawBorder => true;

    [DocOverride("Corner radius applied to the outer frame.", TypeOverride = "Rem", DefaultOverride = "0.75rem")]
    protected virtual Rem BorderRadius => new Rem(0.75f);

    [DocOverride("Stroke width for the outer border.", TypeOverride = "Rem", DefaultOverride = "0.0625rem")]
    protected virtual Rem BorderThickness => new Rem(1f / 16f);

    [DocOverride("Inner padding inside the body region (in addition to the border thickness).", TypeOverride = "EdgeInsets", DefaultOverride = "All(0)")]
    protected virtual EdgeInsets BorderPadding => EdgeInsets.All(new Rem(0f));

    protected override float Margin => 0f;

    [DocOverride("Allow the user to drag any window edge to resize.", TypeOverride = "bool", DefaultOverride = "true")]
    protected virtual bool EdgeResizable => true;

    protected virtual float EdgeResizeThickness => 8f;

    [DocOverride("Minimum allowed window dimensions when edge-resizing.", TypeOverride = "Vector2", DefaultOverride = "(360, 240)")]
    protected virtual Vector2 MinWindowSize => new Vector2(360f, 240f);

    [DocOverride("Toggle maximize when the header is double-clicked.", TypeOverride = "bool", DefaultOverride = "true")]
    protected virtual bool EnableDoubleClickMaximize => true;

    [DocOverride("Per-savegame Scribe key for persisting window position. Null disables persistence.", TypeOverride = "string?", DefaultOverride = "null")]
    protected virtual string? PersistPositionKey => null;

    [DocOverride("Top chrome slot. Return a WindowHeader (or any node) to add a title bar; return null for a chromeless window.", TypeOverride = "LightweaveNode?", DefaultOverride = "null")]
    protected virtual LightweaveNode? Header() {
        return null;
    }

    [DocOverride("Required override returning the body content node tree.", TypeOverride = "LightweaveNode")]
    protected abstract LightweaveNode Body();

    [DocOverride("Bottom chrome slot. Return a WindowFooter (or any node) for a status bar / dialog button row.", TypeOverride = "LightweaveNode?", DefaultOverride = "null")]
    protected virtual LightweaveNode? Footer() {
        return null;
    }

    [DocOverride("Theme slot used to fill the rounded outer frame between header / body / footer.", TypeOverride = "ThemeSlot", DefaultOverride = "SurfaceRaised")]
    protected virtual ThemeSlot OuterFillSlot => ThemeSlot.SurfaceRaised;

    [DocOverride("Theme slot used as the body backdrop when no WindowBody is supplied.", TypeOverride = "ThemeSlot", DefaultOverride = "SurfacePrimary")]
    protected virtual ThemeSlot BodyFillSlot => ThemeSlot.SurfacePrimary;

    [DocOverride("Drag-grab region resolved each frame. Default reads the rect that WindowHeader publishes.", TypeOverride = "Rect?", DefaultOverride = "WindowHeader rect")]
    protected virtual Rect? DragRegion(Rect inRect) {
        Rect? headerRect = LightweaveWindowContext.HeaderRect;
        if (headerRect.HasValue && LightweaveWindowContext.HeaderDraggable) {
            return headerRect.Value;
        }

        return null;
    }

    public override void PreOpen() {
        base.PreOpen();
        drawOwnCloseX = doCloseX;
        doCloseX = false;
        TryRestorePersistedPosition();
    }

    private void TryRestorePersistedPosition() {
        if (positionRestored) {
            return;
        }

        positionRestored = true;
        string? key = PersistPositionKey;
        if (key == null) {
            return;
        }

        LightweaveWindowPositionStore? store = LightweaveWindowPositionStore.GetOrNull();
        if (store == null) {
            return;
        }

        if (!store.TryGet(key, out Rect saved)) {
            return;
        }

        saved.x = Mathf.Clamp(saved.x, 0f, Mathf.Max(0f, Verse.UI.screenWidth - saved.width));
        saved.y = Mathf.Clamp(saved.y, 0f, Mathf.Max(0f, Verse.UI.screenHeight - saved.height));
        saved.width = Mathf.Clamp(saved.width, MinWindowSize.x, Verse.UI.screenWidth);
        saved.height = Mathf.Clamp(saved.height, MinWindowSize.y, Verse.UI.screenHeight);
        windowRect = saved;
    }

    public override void DoWindowContents(Rect inRect) {
        if (EdgeResizable) {
            HandleEdgeResize(inRect);
            UpdateEdgeAbsorb(inRect);
        }

        LightweaveWindowContext.Reset();

        LightweaveRoot.Render(inRect, RootId, BuildRoot, DirectionOverride, ThemeOverride, AfterContent);

        UpdateCursor(inRect);

        HandleWindowDrag(inRect);
    }

    private LightweaveNode BuildRoot() {
        Rem innerR = new Rem(Mathf.Max(0f, BorderRadius.Value - BorderThickness.Value));

        if (DrawBorder) {
            LightweaveWindowContext.RequestHeaderRadius(RadiusSpec.Top(innerR));
            LightweaveWindowContext.RequestFooterRadius(RadiusSpec.Bottom(innerR));
        }
        else {
            LightweaveWindowContext.RequestHeaderRadius(null);
            LightweaveWindowContext.RequestFooterRadius(null);
        }

        LightweaveNode? header = Header();
        LightweaveNode body = Body();
        LightweaveNode? footer = Footer();

        RadiusSpec? bodyRadius = null;
        if (DrawBorder) {
            bool roundTop = header == null;
            bool roundBottom = footer == null;
            if (roundTop && roundBottom) {
                bodyRadius = RadiusSpec.All(innerR);
            }
            else if (roundTop) {
                bodyRadius = RadiusSpec.Top(innerR);
            }
            else if (roundBottom) {
                bodyRadius = RadiusSpec.Bottom(innerR);
            }
        }

        LightweaveNode bodyBacked = Layout.Box.Create(
            padding: BorderPadding,
            background: BackgroundSpec.Of(BodyFillSlot),
            border: null,
            radius: bodyRadius,
            children: c => c.Add(body)
        );

        LightweaveNode stack = Layout.Stack.Create(
            children: s => {
                if (header != null) {
                    s.Add(header);
                }

                s.AddFlex(bodyBacked);

                if (footer != null) {
                    s.Add(footer);
                }
            }
        );

        if (!DrawBorder) {
            return stack;
        }

        return Layout.Box.Create(
            padding: EdgeInsets.All(BorderThickness),
            background: BackgroundSpec.Of(OuterFillSlot),
            border: BorderSpec.All(BorderThickness, ThemeSlot.BorderSubtle),
            radius: RadiusSpec.All(BorderRadius),
            children: c => c.Add(stack)
        );
    }

    private void AfterContent() {
        if (!drawOwnCloseX || LightweaveWindowContext.HeaderOwnsClose) {
            return;
        }

        Rect? header = LightweaveWindowContext.HeaderRect;
        DrawCloseX(header ?? new Rect(0f, 0f, 0f, 0f));
    }

    private void HandleWindowDrag(Rect inRect) {
        if (!draggable) {
            activeDrag = false;
            return;
        }

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
                float now = Time.realtimeSinceStartup;
                bool isDoubleClick = EnableDoubleClickMaximize
                    && lastDragClickTime > 0f
                    && now - lastDragClickTime < 0.3f
                    && (e.mousePosition - lastDragClickPos).sqrMagnitude < 25f;

                if (isDoubleClick) {
                    ToggleMaximized();
                    lastDragClickTime = -1f;
                    e.Use();
                    return;
                }

                lastDragClickTime = now;
                lastDragClickPos = e.mousePosition;
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

    private void ToggleMaximized() {
        if (isMaximized) {
            windowRect = prerestoreRect;
            isMaximized = false;
        }
        else {
            prerestoreRect = windowRect;
            windowRect = new Rect(0f, 0f, Verse.UI.screenWidth, Verse.UI.screenHeight);
            isMaximized = true;
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
            }
            else {
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
            }
            else {
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
            CursorOverrides.RestoreDefault();
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

    private void DrawCloseX(Rect anchor) {
        const float padding = 12f;
        const float size = 18f;
        Rect closeRect = new Rect(
            anchor.xMax - size - padding,
            anchor.y + padding,
            size,
            size
        );
        LightweaveHitTracker.Track(closeRect);

        Theme.Theme theme = ThemeOverride ?? ThemeRegistry.Default;
        Color accent = theme.GetColor(ThemeSlot.SurfaceAccent);
        accent.a = 1f;
        Color baseColor = theme.GetColor(ThemeSlot.TextPrimary);
        Color hoverColor = accent;

        if (Widgets.ButtonImage(closeRect, TexButton.CloseXSmall, baseColor, hoverColor, true, null)) {
            Close();
        }

        MouseoverSounds.DoRegion(closeRect);
    }


    public override void PostClose() {
        if (currentCursor != null) {
            CursorOverrides.RestoreDefault();
            currentCursor = null;
        }

        TryPersistPosition();

        LightweaveRoot.Release(RootId);
        base.PostClose();
    }

    private void TryPersistPosition() {
        string? key = PersistPositionKey;
        if (key == null) {
            return;
        }

        LightweaveWindowPositionStore? store = LightweaveWindowPositionStore.GetOrNull();
        if (store == null) {
            return;
        }

        Rect rectToStore = isMaximized ? prerestoreRect : windowRect;
        store.Set(key, rectToStore);
    }
}
