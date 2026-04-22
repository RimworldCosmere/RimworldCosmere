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

    public static void Render(Rect inRect, Guid rootId, Func<LightweaveNode> build)
    {
        if (!stores.TryGetValue(rootId, out HookStore store))
        {
            store = new HookStore();
            stores[rootId] = store;
        }

        RenderContext ctx = new RenderContext { Hooks = store };
        ctx.ThemeStack.Push(GetBaseTheme());
        ctx.DirectionStack.Push(DetectDirection());
        ctx.PointerPos = Event.current?.mousePosition ?? Vector2.zero;
        RenderContext.Push(ctx);
        try
        {
            LightweaveNode root = build();
            root.MeasuredRect = inRect;
            root.ContentRect = inRect;
            Paint(root);
            foreach (Action overlay in ctx.DeferredOverlays)
            {
                overlay();
            }
            store.RetireUntouched();
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

    private static void Paint(LightweaveNode node)
    {
        node.Paint?.Invoke(node.MeasuredRect);
        foreach (LightweaveNode child in node.Children)
        {
            Paint(child);
        }
    }
}
