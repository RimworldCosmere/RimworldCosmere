using Cosmere.Core.UI;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.UI.Dock;

public abstract class DockSectionBase : IDockSection {
    /// Folded groups, by label key. Session-only - a fold is a glance, not a setting.
    private readonly HashSet<string> collapsedGroups = new HashSet<string>();

    public abstract string SystemId { get; }

    public ISystemSkin Skin => SystemSkinRegistry.ForOrFallback(SystemId);

    public abstract float GetHeaderHeight();

    public abstract float GetExpandedBodyHeight(Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx);

    // one skin per section, so the underline is built once, not every frame
    private Color? underline;

    public virtual void DrawHeader(Rect rect, bool expanded) {
        Color accent = Skin.AccentColor;
        underline ??= new Color(accent.r, accent.g, accent.b, 0.55f);
        Widgets.DrawBoxSolid(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), underline.Value);

        Rect chevronRect = new Rect(rect.xMax - 18f, rect.y, 14f, rect.height);
        UIText.EllipsisLabel(chevronRect, expanded ? "-" : "+", Skin.HeaderFont, TextAnchor.MiddleCenter, Skin.HeaderTextColor);

        Rect labelRect = new Rect(rect.x + 10f, rect.y, chevronRect.x - rect.x - 14f, rect.height);
        UIText.EllipsisLabel(labelRect, Skin.HeaderLabel, Skin.HeaderFont, TextAnchor.MiddleLeft, Skin.HeaderTextColor);
        Widgets.DrawHighlightIfMouseover(rect);
    }

    public abstract void DrawBody(Rect rect, Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx);

    protected bool IsGroupCollapsed(string groupKey) {
        return collapsedGroups.Contains(groupKey);
    }

    /// Returns true when the click folded the group, so the caller can close whatever was open inside it.
    protected bool ToggleGroupFold(string groupKey) {
        bool folded = collapsedGroups.Add(groupKey);
        if (!folded) collapsedGroups.Remove(groupKey);

        RimWorld.SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
        return folded;
    }

    // A section with nothing to pin costs a zero and a skipped rect.
    public virtual float GetPinnedHeight(Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) => 0f;

    public virtual void DrawPinned(Rect rect, Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) { }
}
