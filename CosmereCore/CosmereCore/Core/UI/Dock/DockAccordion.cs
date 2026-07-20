using Cosmere.Core.UI.Model;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.UI.Dock;

public sealed class DockAccordion {
    private Vector2 scrollPos;
    public string? ExpandedSystemId { get; set; }

    public void Draw(
        Rect rect,
        Pawn pawn,
        IReadOnlyList<InvestitureSnapshot> snapshots,
        DockRenderContext ctx
    ) {
        if (snapshots.Count == 0) return;

        ExpandedSystemId ??= snapshots[0].SystemId;

        float y = rect.y;
        for (int i = 0; i < snapshots.Count; i++) {
            InvestitureSnapshot snap = snapshots[i];
            IDockSection? section = DockSectionRegistry.For(snap.SystemId);
            if (section == null) continue;

            bool isExpanded = section.SystemId == ExpandedSystemId;

            float headerHeight = section.GetHeaderHeight();
            Rect headerRect = new Rect(rect.x, y, rect.width, headerHeight);
            section.DrawHeader(headerRect, isExpanded);

            if (Widgets.ButtonInvisible(headerRect)) {
                ExpandedSystemId = isExpanded ? null : section.SystemId;
                scrollPos = Vector2.zero;
                RimWorld.SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
            }

            y += headerHeight;

            if (!isExpanded) continue;

            float bodyHeight = section.GetExpandedBodyHeight(pawn, snap, ctx);
            float availableHeight = rect.yMax - y;
            Rect bodyRect = new Rect(rect.x, y, rect.width, Mathf.Min(bodyHeight, availableHeight));
            if (bodyHeight > availableHeight) {
                Rect viewRect = new Rect(0f, 0f, rect.width - 16f, bodyHeight);
                Widgets.BeginScrollView(bodyRect, ref scrollPos, viewRect);
                section.DrawBody(new Rect(0f, 0f, viewRect.width, bodyHeight), pawn, snap, ctx);
                Widgets.EndScrollView();
            }
            else {
                section.DrawBody(bodyRect, pawn, snap, ctx);
            }

            y += bodyRect.height;
        }
    }
}