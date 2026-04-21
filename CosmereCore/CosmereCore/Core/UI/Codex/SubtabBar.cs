using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Codex;

public static class SubtabBar {
    private static readonly (CodexSubtab tab, string labelKey)[] entries = [
        (CodexSubtab.Autocast, "CC_Codex_Subtab_Autocast"),
        (CodexSubtab.Progression, "CC_Codex_Subtab_Progression"),
        (CodexSubtab.Bonded, "CC_Codex_Subtab_Bonded"),
        (CodexSubtab.Memories, "CC_Codex_Subtab_Memories"),
    ];

    public static void Draw(Rect rect, CodexState state, Color accent) {
        float tabWidth = rect.width / entries.Length;
        for (int i = 0; i < entries.Length; i++) {
            Rect tab = new Rect(rect.x + (i * tabWidth), rect.y, tabWidth, rect.height);
            bool selected = state.Subtab == entries[i].tab;
            Color bg = selected ? new Color(0.18f, 0.18f, 0.22f, 0.95f) : new Color(0.1f, 0.1f, 0.12f, 0.65f);
            Widgets.DrawBoxSolid(tab, bg);
            Widgets.DrawBox(tab);

            if (selected) {
                Widgets.DrawBoxSolid(new Rect(tab.x, tab.yMax - 2f, tab.width, 2f), accent);
            }

            using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, selected ? Color.white : new Color(0.75f, 0.75f, 0.75f)))
                Widgets.Label(tab, entries[i].labelKey.Translate());

            if (Widgets.ButtonInvisible(tab)) {
                state.Subtab = entries[i].tab;
            }
        }
    }
}
