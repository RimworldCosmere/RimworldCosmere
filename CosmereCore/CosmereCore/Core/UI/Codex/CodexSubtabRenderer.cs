using UnityEngine;
using Verse;
using Cosmere.Core.UI.Model;

namespace Cosmere.Core.UI.Codex;

public static class CodexSubtabRenderer {
    private static readonly Color emptyLabelColor = new Color(0.7f, 0.7f, 0.7f);

    public static void Draw(
        Rect rect,
        Pawn pawn,
        CodexState state,
        IInvestitureProvider active,
        CodexSubtab subtab
    ) {
        ICodexContentProvider cp = active.Codex;
        switch (subtab) {
            case CodexSubtab.Progression when cp.HasProgression(pawn):
                cp.DrawProgression(rect, pawn, state);
                return;
            case CodexSubtab.Bonds when cp.HasBonds(pawn):
                cp.DrawBonds(rect, pawn, state);
                return;
            case CodexSubtab.Memories when cp.HasMemories(pawn):
                cp.DrawMemories(rect, pawn, state);
                return;
        }

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, emptyLabelColor))
            Widgets.Label(rect, EmptyLabelKey(subtab).Translate());
    }

    private static string EmptyLabelKey(CodexSubtab subtab) {
        return subtab switch {
            CodexSubtab.Progression => "CC_Codex_Progression_Empty",
            CodexSubtab.Bonds => "CC_Codex_Bonds_Empty",
            CodexSubtab.Memories => "CC_Codex_Memories_Empty",
            _ => "CC_Codex_Subtab_Empty",
        };
    }
}
