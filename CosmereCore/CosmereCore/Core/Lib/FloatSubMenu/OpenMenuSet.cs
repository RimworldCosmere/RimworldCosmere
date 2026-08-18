using UnityEngine;
using Verse;

namespace Cosmere.Core.Lib.FloatSubMenu;

internal class OpenMenuSet {
    private static readonly Dictionary<FloatMenu, OpenMenuSet> sets =
        new Dictionary<FloatMenu, OpenMenuSet>();

    private readonly List<FloatMenu> menus = new List<FloatMenu>();
    private readonly List<Rect> rects = new List<Rect>();
    private float cachedDistance;
    private Vector2 cachedPosition;

    private bool cacheValid;

    private OpenMenuSet(FloatMenu parent, FloatSubMenuInner child) {
        Add(parent);
        Add(child);
    }

    public float MinDistance {
        get {
            Vector2 pos = Verse.UI.MousePositionOnUIInverted;
            if (!cacheValid || pos != cachedPosition) {
                cacheValid = true;
                cachedPosition = pos;
                cachedDistance = rects.Min(r => GenUI.DistFromRect(r, pos));
            }

            return cachedDistance;
        }
    }

    private void Add(FloatMenu menu) {
        if (!menus.Contains(menu)) {
            menus.Add(menu);
            rects.Add(menu.windowRect.ContractedBy(-5f));
        }

        sets[menu] = this;
        cacheValid = false;
    }

    private void Remove(int i) {
        sets.Remove(menus[i]);
        menus.RemoveAt(i);
        rects.RemoveAt(i);
        cacheValid = false;
    }

    private void Remove(FloatMenu menu) {
        int i = menus.IndexOf(menu);
        if (i > 0) {
            Remove(i);
        }

        if (menus.Count == 1 || i == 0) {
            for (int j = menus.Count - 1; j >= 0; j--) {
                Remove(j);
            }
        }
    }

    public static OpenMenuSet? For(FloatMenu menu) {
        return sets.TryGetValue(menu, out OpenMenuSet? set) ? set : null;
    }

    public static void Open(FloatMenu parent, FloatSubMenuInner child) {
        if (sets.TryGetValue(parent, out OpenMenuSet? set)) {
            set.Add(child);
        } else {
            // the constructor already registers both menus; this write is redundant but explicit.
            sets[parent] = new OpenMenuSet(parent, child);
        }
    }

    public static void Close(FloatMenu menu) {
        if (sets.TryGetValue(menu, out OpenMenuSet? set)) {
            set.Remove(menu);
        }
    }
}
