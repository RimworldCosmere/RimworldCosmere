using Cosmere.Core.UI.Model;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Codex;

public static class BondsSubtabRenderer {
    public static void Draw(Rect rect, Pawn pawn, IInvestitureProvider active, CodexState state) {
        if (active is ICodexContentProvider cp && cp.HasBonds(pawn)) {
            cp.DrawBonds(pawn, rect, state);
            return;
        }

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, new Color(0.7f, 0.7f, 0.7f)))
            Widgets.Label(rect, "CC_Codex_Bonds_Empty".Translate());
    }
}