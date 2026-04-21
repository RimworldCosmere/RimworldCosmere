using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Dock;

public sealed class PlaceholderDockSection : IDockSection {
    public string SystemId { get; }
    public ISystemSkin Skin { get; }

    public PlaceholderDockSection(string systemId) {
        SystemId = systemId;
        Skin = SystemSkinRegistry.For(systemId);
    }

    public float GetHeaderHeight() => 28f;

    public float GetExpandedBodyHeight(
        Pawn pawn,
        InvestitureSnapshot snapshot,
        DockRenderContext ctx
    ) => 40f;

    public void DrawHeader(Rect rect, bool expanded) {
        Widgets.DrawBoxSolid(rect, new Color(Skin.AccentColor.r, Skin.AccentColor.g, Skin.AccentColor.b, 0.25f));
        using (new TextBlock(Skin.HeaderFont, TextAnchor.MiddleLeft, Skin.HeaderTextColor))
            Widgets.Label(rect.ContractedBy(6f, 0f), Skin.HeaderLabel + (expanded ? " -" : " +"));
    }

    public void DrawBody(
        Rect rect,
        Pawn pawn,
        InvestitureSnapshot snapshot,
        DockRenderContext ctx
    ) {
        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, Color.gray))
            Widgets.Label(rect, "(no cells yet)");
    }
}
