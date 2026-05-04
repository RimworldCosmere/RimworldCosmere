using System;
using Cosmere.Lightweave.Theme;
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

    private static void Paint(LightweaveNode node) {
        Action paintChildren = () => {
            foreach (LightweaveNode child in node.Children) {
                Paint(child);
            }
        };
        if (node.Paint != null) {
            node.Paint(node.MeasuredRect, paintChildren);
        }
        else {
            paintChildren();
        }
    }
}