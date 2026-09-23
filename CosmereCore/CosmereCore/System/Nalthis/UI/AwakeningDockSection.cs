using Cosmere.Core.UI.Dock;
using Cosmere.Core.UI.Model;
using UnityEngine;
using Verse;

namespace Cosmere.System.Nalthis.UI;

public sealed class AwakeningDockSection : DockSectionBase {
    public override string SystemId => "Awakening";

    public override float GetHeaderHeight() {
        return 28f;
    }

    public override float GetExpandedBodyHeight(Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        return 28f;
    }

    public override void DrawBody(Rect rect, Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, new Color(0.8f, 0.7f, 0.7f)))
            Widgets.Label(rect, "CC_Dock_Awakening_NoBreaths".Translate());
    }
}
