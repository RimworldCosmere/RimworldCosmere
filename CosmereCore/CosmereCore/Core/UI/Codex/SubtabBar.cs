using System;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Model;
using UnityEngine;
using Verse;
using Verse.Sound;

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

        if (active.Codex.ShowsMemoriesSubtab) {
            buffer.Add((CodexSubtab.Memories, "CC_Codex_Subtab_Memories"));
        }

        if (!buffer.Exists(e => e.tab == state.Subtab)) {
            state.Subtab = buffer[0].tab;
        }

        if (Draw(rect, buffer, state.Subtab, accent, out CodexSubtab clicked)) {
            state.Subtab = clicked;
        }
    }

    public static bool Draw<T>(
        Rect rect,
        IReadOnlyList<(T tab, string labelKey)> tabs,
        T selected,
        Color accent,
        out T clicked
    )
        where T : struct, Enum {
        clicked = selected;
        float tabWidth = rect.width / tabs.Count;
        for (int i = 0; i < tabs.Count; i++) {
            Rect tab = new Rect(rect.x + i * tabWidth, rect.y, tabWidth, rect.height);
            bool isSelected = EqualityComparer<T>.Default.Equals(selected, tabs[i].tab);

            if (!isSelected && Mouse.IsOver(tab)) {
                Widgets.DrawBoxSolid(tab, new Color(1f, 1f, 1f, 0.04f));
            }

            string label = tabs[i].labelKey.Translate();
            Color textColor = isSelected ? new Color(0.88f, 0.73f, 0.42f) : new Color(0.55f, 0.50f, 0.41f);
            UIText.EllipsisLabel(tab.ContractedBy(4f, 0f), label, GameFont.Small, TextAnchor.MiddleCenter, textColor);

            if (isSelected) {
                // Flush at the frame ends - an underline stopping short there left a visible notch.
                const float inset = 6f;
                float left = i == 0 ? 0f : inset;
                float right = i == tabs.Count - 1 ? 0f : inset;
                float width = tab.width - left - right;

                Rect glow = new Rect(tab.x + left, tab.yMax - 4f, width, 4f);
                Widgets.DrawBoxSolid(glow, new Color(accent.r, accent.g, accent.b, 0.18f));

                Rect underline = new Rect(tab.x + left, tab.yMax - 2f, width, 2f);
                Widgets.DrawBoxSolid(underline, accent);
            }

            MouseoverSounds.DoRegion(tab);
            if (Widgets.ButtonInvisible(tab)) {
                clicked = tabs[i].tab;
                return true;
            }
        }

        return false;
    }
}
