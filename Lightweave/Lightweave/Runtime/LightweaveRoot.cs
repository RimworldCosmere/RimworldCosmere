using System;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Theme;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Runtime;

public static class LightweaveRoot {
    private static readonly Dictionary<Guid, HookStore> stores = new Dictionary<Guid, HookStore>();

    public static void Render(
        Rect inRect,
        Guid rootId,
        Func<LightweaveNode> build,
        Direction? directionOverride = null,
        Theme.Theme? themeOverride = null,
        Action? afterContent = null
    ) {
        if (!stores.TryGetValue(rootId, out HookStore store)) {
            store = new HookStore();
            stores[rootId] = store;
        }

        AnimationClock.ClearFrame();
        LightweaveHitTracker.Clear();
        RenderContext ctx = new RenderContext(store) { RootId = rootId, RootRect = inRect };
        ctx.ThemeStack.Push(themeOverride ?? GetBaseTheme());
        ctx.DirectionStack.Push(directionOverride ?? DetectDirection());
        ctx.Breakpoint = Breakpoints.For(inRect.width);
        ctx.PointerPos = Event.current?.mousePosition ?? Vector2.zero;
        RenderContext.Push(ctx);
        try {
            try {
                LightweaveNode root = build();
                root.MeasuredRect = inRect;
                root.ContentRect = inRect;
                Paint(root);
                afterContent?.Invoke();
                ctx.FlushHotkeys();
                ctx.PendingOverlays.Flush();
                CursorOverrides.ApplyForFrame();
            }
            finally {
                ctx.PendingOverlays.Clear();
                store.RetireUntouched();
            }
        }
        finally {
            RenderContext.Clear();
        }
    }

    public static void Release(Guid rootId) {
        if (stores.TryGetValue(rootId, out HookStore store)) {
            store.ReleaseAll();
            stores.Remove(rootId);
        }
    }

    private static Theme.Theme GetBaseTheme() {
        return ThemeRegistry.Default;
    }

    private static Direction DetectDirection() {
        string code = LanguageDatabase.activeLanguage?.folderName ?? "English";
        return code is "Arabic" or "Hebrew" or "Persian" or "Urdu" ? Direction.Rtl : Direction.Ltr;
    }

    public static void PaintSubtree(LightweaveNode node, Rect rect) {
        node.MeasuredRect = rect;
        node.ContentRect = rect;
        Paint(node);
    }


    private static LightweaveNode? currentPaintNode;

    private static readonly Action SharedPaintChildren = () => {
        LightweaveNode? node = currentPaintNode;
        if (node == null) {
            return;
        }

        for (int i = 0; i < node.Children.Count; i++) {
            Paint(node.Children[i]);
        }
    };

    private static void Paint(LightweaveNode node) {
        RenderContext? rc = RenderContext.CurrentOrNull;
        int previousParentHash = rc?.ParentPathHash ?? 0;
        if (rc != null) {
            rc.ParentPathHash = node.BuildParentPathHash;
        }

        LightweaveNode? prevPaintNode = currentPaintNode;
        currentPaintNode = node;

        Style style = default;
        bool hasStyle = node.Style.HasValue || (node.Classes != null && node.Classes.Length > 0);
        if (hasStyle && rc != null) {
            style = rc.Theme.ResolveStyle(node);
        }

        if (style.Visible == false || style.Display == Display.None) {
            currentPaintNode = prevPaintNode;
            if (rc != null) {
                rc.ParentPathHash = previousParentHash;
            }
            return;
        }

        static float ClampMin(float v, Rem? min) {
            if (min.HasValue) {
                float minPx = min.Value.ToPixels();
                if (v < minPx) {
                    return minPx;
                }
            }
            return v;
        }

        static float ClampMax(float v, Rem? max) {
            if (max.HasValue) {
                float maxPx = max.Value.ToPixels();
                if (v > maxPx) {
                    return maxPx;
                }
            }
            return v;
        }

        Position pos = style.Position ?? Position.Static;
        if (pos == Position.Absolute || pos == Position.Fixed) {
            Rect ancestor;
            if (pos == Position.Fixed) {
                ancestor = rc?.RootRect ?? node.MeasuredRect;
            }
            else if (rc != null && rc.PositioningAncestorStack.Count > 0) {
                ancestor = rc.PositioningAncestorStack.Peek();
            }
            else {
                ancestor = rc?.RootRect ?? node.MeasuredRect;
            }

            float w = style.Width?.ToPixels()
                      ?? node.MeasureWidth?.Invoke()
                      ?? ancestor.width;
            w = ClampMin(w, style.MinWidth);
            w = ClampMax(w, style.MaxWidth);

            float h = style.Height?.ToPixels()
                      ?? node.Measure?.Invoke(w)
                      ?? node.PreferredHeight
                      ?? 0f;
            h = ClampMin(h, style.MinHeight);
            h = ClampMax(h, style.MaxHeight);

            float x;
            if (style.Left.HasValue) {
                x = ancestor.x + style.Left.Value.ToPixels();
            }
            else if (style.Right.HasValue) {
                x = ancestor.x + ancestor.width - style.Right.Value.ToPixels() - w;
            }
            else {
                x = ancestor.x;
            }

            float y;
            if (style.Top.HasValue) {
                y = ancestor.y + style.Top.Value.ToPixels();
            }
            else if (style.Bottom.HasValue) {
                y = ancestor.y + ancestor.height - style.Bottom.Value.ToPixels() - h;
            }
            else {
                y = ancestor.y;
            }

            Rect outer = new Rect(x, y, w, h);
            if (style.Margin.HasValue) {
                outer = style.Margin.Value.Shrink(outer, rc?.Direction ?? Direction.Ltr);
            }
            node.MeasuredRect = outer;
        }
        else {
            Rect r = node.MeasuredRect;
            if (style.Margin.HasValue) {
                r = style.Margin.Value.Shrink(r, rc?.Direction ?? Direction.Ltr);
            }

            if (style.Width.HasValue) {
                r.width = style.Width.Value.ToPixels();
            }
            if (style.Height.HasValue) {
                r.height = style.Height.Value.ToPixels();
            }
            float cw = ClampMin(r.width, style.MinWidth);
            cw = ClampMax(cw, style.MaxWidth);
            float ch = ClampMin(r.height, style.MinHeight);
            ch = ClampMax(ch, style.MaxHeight);
            r.width = cw;
            r.height = ch;

            if (pos == Position.Relative) {
                if (style.Left.HasValue) {
                    r.x += style.Left.Value.ToPixels();
                }
                else if (style.Right.HasValue) {
                    r.x -= style.Right.Value.ToPixels();
                }
                if (style.Top.HasValue) {
                    r.y += style.Top.Value.ToPixels();
                }
                else if (style.Bottom.HasValue) {
                    r.y -= style.Bottom.Value.ToPixels();
                }
            }

            node.MeasuredRect = r;
        }

        bool pushedAncestor = false;
        if (rc != null && pos != Position.Static && (pos == Position.Relative || pos == Position.Absolute || pos == Position.Fixed)) {
            rc.PositioningAncestorStack.Push(node.MeasuredRect);
            pushedAncestor = true;
        }

        Color savedColor = GUI.color;
        bool appliedOpacity = false;
        if (style.Opacity.HasValue) {
            GUI.color = new Color(savedColor.r, savedColor.g, savedColor.b, savedColor.a * style.Opacity.Value);
            appliedOpacity = true;
        }

        try {
            if (style.Background != null || style.Border.HasValue || style.Radius.HasValue) {
                PaintBox.Draw(node.MeasuredRect, style.Background, style.Border, style.Radius);
            }

            Rect innerRect = node.MeasuredRect;
            if (style.Padding.HasValue) {
                innerRect = style.Padding.Value.Shrink(innerRect, rc?.Direction ?? Direction.Ltr);
            }

            if (node.Paint != null) {
                node.Paint(innerRect, SharedPaintChildren);
            }
            else {
                for (int i = 0; i < node.Children.Count; i++) {
                    Paint(node.Children[i]);
                }
            }
        }
        finally {
            if (appliedOpacity) {
                GUI.color = savedColor;
            }
            if (pushedAncestor && rc != null) {
                rc.PositioningAncestorStack.Pop();
            }
            currentPaintNode = prevPaintNode;
            if (rc != null) {
                rc.ParentPathHash = previousParentHash;
            }
        }
    }
}