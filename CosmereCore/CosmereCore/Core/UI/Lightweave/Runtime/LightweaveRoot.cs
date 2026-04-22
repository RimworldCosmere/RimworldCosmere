using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Cosmere.Core.UI.Lightweave.Theme;
using Cosmere.Core.UI.Lightweave.Types;
using Cosmere.Core.UI.Lightweave.Fonts;

namespace Cosmere.Core.UI.Lightweave.Runtime;

public static class LightweaveRoot
{
    private static readonly Dictionary<Guid, HookStore> stores = new Dictionary<Guid, HookStore>();
    private static Theme.Theme? baseTheme;

    public static void Render(Rect inRect, Guid rootId, Func<LightweaveNode> build, Direction? directionOverride = null)
    {
        if (!stores.TryGetValue(rootId, out HookStore store))
        {
            store = new HookStore();
            stores[rootId] = store;
        }

        RenderContext ctx = new RenderContext { Hooks = store };
        ctx.ThemeStack.Push(GetBaseTheme());
        ctx.DirectionStack.Push(directionOverride ?? DetectDirection());
        ctx.PointerPos = Event.current?.mousePosition ?? Vector2.zero;
        RenderContext.Push(ctx);
        try
        {
            try
            {
                LightweaveNode root = build();
                root.MeasuredRect = inRect;
                root.ContentRect = inRect;
                Paint(root);
                ctx.PendingOverlays.Flush();
            }
            finally
            {
                ctx.PendingOverlays.Clear();
                store.RetireUntouched();
            }
        }
        finally
        {
            RenderContext.Clear();
        }
    }

    public static void Release(Guid rootId)
    {
        if (stores.TryGetValue(rootId, out HookStore store))
        {
            store.ReleaseAll();
            stores.Remove(rootId);
        }
    }

    private static Theme.Theme GetBaseTheme()
    {
        if (baseTheme != null)
        {
            return baseTheme;
        }
        Font body = LightweaveFonts.ArimoRegular;
        Font bold = LightweaveFonts.ArimoBold;
        Font heading = LightweaveFonts.ArimoBold;
        Font display = LightweaveFonts.CarlitoBold;
        Font mono = LightweaveFonts.JetBrainsMono;
        baseTheme = BaseTheme.Build(body, bold, heading, display, mono);
        return baseTheme;
    }

    private static Direction DetectDirection()
    {
        string code = LanguageDatabase.activeLanguage?.folderName ?? "English";
        return code is "Arabic" or "Hebrew" or "Persian" or "Urdu" ? Direction.Rtl : Direction.Ltr;
    }

    public static void PaintSubtree(LightweaveNode node, Rect rect)
    {
        node.MeasuredRect = rect;
        node.ContentRect = rect;
        Paint(node);
    }

    private static void Paint(LightweaveNode node)
    {
        Action paintChildren = () =>
        {
            foreach (LightweaveNode child in node.Children)
            {
                Paint(child);
            }
        };
        if (node.Paint != null)
        {
            node.Paint(node.MeasuredRect, paintChildren);
        }
        else
        {
            paintChildren();
        }
    }
}
