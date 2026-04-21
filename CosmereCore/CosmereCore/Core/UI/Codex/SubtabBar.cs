using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Codex;

public static class SubtabBar {
    private static readonly (CodexSubtab tab, string label)[] entries = [
        (CodexSubtab.Autocast, "Autocast"),
        (CodexSubtab.Progression, "Progression"),
        (CodexSubtab.Bonded, "Bonded"),
        (CodexSubtab.Memories, "Memories"),
    ];

    public static void Draw(Rect rect, CodexState state) {
        float tabWidth = rect.width / entries.Length;
        for (int i = 0; i < entries.Length; i++) {
            Rect tab = new Rect(rect.x + (i * tabWidth), rect.y, tabWidth, rect.height);
            bool selected = state.Subtab == entries[i].tab;
            Color bg = selected ? new Color(0.18f, 0.18f, 0.22f, 0.95f) : new Color(0.1f, 0.1f, 0.12f, 0.65f);
            Widgets.DrawBoxSolid(tab, bg);
            Widgets.DrawBox(tab);

            using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, selected ? Color.white : new Color(0.75f, 0.75f, 0.75f)))
                Widgets.Label(tab, entries[i].label);

            if (Widgets.ButtonInvisible(tab)) {
                state.Subtab = entries[i].tab;
            }
        }
    }
}
