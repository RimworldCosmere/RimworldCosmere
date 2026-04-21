using Cosmere.Core.UI.Model;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.UI.Dock;

public sealed class DockAccordion {
    public string? ExpandedSystemId { get; set; }

    public void Draw(
        Rect rect,
        Verse.Pawn pawn,
        List<InvestitureSnapshot> snapshots,
        DockRenderContext ctx
    ) {
        if (snapshots.Count == 0) return;

        string defaultExpanded = ExpandedSystemId ?? snapshots[0].SystemId;
        if (ExpandedSystemId == null) ExpandedSystemId = defaultExpanded;

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
                RimWorld.SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
            }
            y += headerHeight;

            if (!isExpanded) continue;

            float bodyHeight = section.GetExpandedBodyHeight(pawn, snap, ctx);
            float availableHeight = rect.yMax - y;
            float drawnHeight = Mathf.Min(bodyHeight, availableHeight);
            Rect bodyRect = new Rect(rect.x, y, rect.width, drawnHeight);
            section.DrawBody(bodyRect, pawn, snap, ctx);
            y += drawnHeight;
        }
    }
}
