using Cosmere.Core.UI.Model;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Codex;

public static class ProgressionSubtabRenderer {
    public static void Draw(Rect rect, Pawn pawn, IInvestitureProvider active) {
        if (active is ICodexContentProvider cp && cp.HasProgression(pawn)) {
            cp.DrawProgression(pawn, rect);
            return;
        }
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, new Color(0.7f, 0.7f, 0.7f)))
            Widgets.Label(rect, "No progression data for this system yet.");
    }
}
