using Cosmere.Core.UI.Model;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Codex;

public static class SubtabBar {
    private static readonly List<(CodexSubtab tab, string labelKey)> buffer =
        new List<(CodexSubtab tab, string labelKey)>();

    public static void Draw(Rect rect, Pawn pawn, CodexState state, IInvestitureProvider active, Color accent) {
        buffer.Clear();
        buffer.Add((CodexSubtab.Autocast, "CC_Codex_Subtab_Autocast"));
        buffer.Add((CodexSubtab.Progression, "CC_Codex_Subtab_Progression"));
        if (active.Codex.ShowsBondsSubtab) {
            buffer.Add((CodexSubtab.Bonds, "CC_Codex_Subtab_Bonds"));
        }

        buffer.Add((CodexSubtab.Memories, "CC_Codex_Subtab_Memories"));

        if (!buffer.Exists(e => e.tab == state.Subtab)) {
            state.Subtab = buffer[0].tab;
        }

        float tabWidth = rect.width / buffer.Count;
        for (int i = 0; i < buffer.Count; i++) {
            Rect tab = new Rect(rect.x + i * tabWidth, rect.y, tabWidth, rect.height);
            bool selected = state.Subtab == buffer[i].tab;
            Color bg = selected ? new Color(0.18f, 0.18f, 0.22f, 0.95f) : new Color(0.1f, 0.1f, 0.12f, 0.65f);
            Widgets.DrawBoxSolid(tab, bg);
            Widgets.DrawBox(tab);

            if (selected) {
                Widgets.DrawBoxSolid(new Rect(tab.x, tab.yMax - 2f, tab.width, 2f), accent);
            }

            using (new TextBlock(
                       GameFont.Small,
                       TextAnchor.MiddleCenter,
                       selected ? Color.white : new Color(0.75f, 0.75f, 0.75f)
                   ))
                Widgets.Label(tab, buffer[i].labelKey.Translate());

            if (Widgets.ButtonInvisible(tab)) {
                state.Subtab = buffer[i].tab;
            }
        }
    }
}