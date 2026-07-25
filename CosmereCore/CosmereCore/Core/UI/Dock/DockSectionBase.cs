using Cosmere.Core.UI;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Dock;

public abstract class DockSectionBase : IDockSection {
    public abstract string SystemId { get; }
    public ISystemSkin Skin => SystemSkinRegistry.ForOrFallback(SystemId);

    public abstract float GetHeaderHeight();

    public abstract float GetExpandedBodyHeight(Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx);

    public virtual void DrawHeader(Rect rect, bool expanded) {
        Widgets.DrawBoxSolid(rect, new Color(Skin.AccentColor.r, Skin.AccentColor.g, Skin.AccentColor.b, 0.16f));
        Widgets.DrawBoxSolid(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), new Color(Skin.AccentColor.r, Skin.AccentColor.g, Skin.AccentColor.b, 0.55f));

        Rect chevronRect = new Rect(rect.xMax - 18f, rect.y, 14f, rect.height);
        UIText.EllipsisLabel(chevronRect, expanded ? "-" : "+", Skin.HeaderFont, TextAnchor.MiddleCenter, Skin.HeaderTextColor);

        Rect labelRect = new Rect(rect.x + 10f, rect.y, chevronRect.x - rect.x - 14f, rect.height);
        UIText.EllipsisLabel(labelRect, Skin.HeaderLabel, Skin.HeaderFont, TextAnchor.MiddleLeft, Skin.HeaderTextColor);
        Widgets.DrawHighlightIfMouseover(rect);
    }

    public abstract void DrawBody(Rect rect, Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx);
}
