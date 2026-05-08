using System;
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

        try {
            if (node.Paint != null) {
                node.Paint(node.MeasuredRect, SharedPaintChildren);
            }
            else {
                for (int i = 0; i < node.Children.Count; i++) {
                    Paint(node.Children[i]);
                }
            }
        }
        finally {
            currentPaintNode = prevPaintNode;
            if (rc != null) {
                rc.ParentPathHash = previousParentHash;
            }
        }
    }
}