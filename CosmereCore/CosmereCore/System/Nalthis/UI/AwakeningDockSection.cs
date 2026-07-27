using Cosmere.Core.UI.Dock;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;

namespace Cosmere.System.Nalthis.UI;

public sealed class AwakeningDockSection : IDockSection {
    public string SystemId => "Awakening";

    public ISystemSkin Skin => SystemSkinRegistry.ForOrFallback(SystemId);

    public float GetHeaderHeight() {
        return 28f;
    }

    public float GetExpandedBodyHeight(Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        return 28f;
    }

    public void DrawHeader(Rect rect, bool expanded) {
        Widgets.DrawBoxSolid(rect, new Color(Skin.AccentColor.r, Skin.AccentColor.g, Skin.AccentColor.b, 0.25f));
        using (new TextBlock(Skin.HeaderFont, TextAnchor.MiddleLeft, Skin.HeaderTextColor))
            Widgets.Label(rect.ContractedBy(6f, 0f), Skin.HeaderLabel + (expanded ? " -" : " +"));
    }

    public void DrawBody(Rect rect, Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, new Color(0.8f, 0.7f, 0.7f)))
            Widgets.Label(rect, "CC_Dock_Awakening_NoBreaths".Translate());
    }
}
