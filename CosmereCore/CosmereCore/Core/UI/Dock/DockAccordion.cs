using Cosmere.Core.Settings;
using Cosmere.Core.UI.Model;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.UI.Dock;

public sealed class DockAccordion {
    private const float BodyPadX = 4f;
    private Vector2 scrollPos;
    private bool defaulted;

    public string? ExpandedSystemId { get; set; }

    public void Draw(
        Rect rect,
        Pawn pawn,
        IReadOnlyList<InvestitureSnapshot> snapshots,
        DockRenderContext ctx
    ) {
        if (snapshots.Count == 0) return;

        // Only picks a default once - re-running every frame reopened whatever the player just closed.
        if (!defaulted) {
            defaulted = true;
            ExpandedSystemId ??= snapshots[0].SystemId;
        }

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

            // Two ceilings, the lower one wins - without the settings cap, a large section runs off a short screen.
            float availableHeight = Mathf.Min(
                rect.yMax - y,
                Mod.GetModSettings<CoreModSettings>().dockSectionMaxHeight
            );
            Rect bodyRect = new Rect(rect.x + BodyPadX, y, rect.width - BodyPadX * 2f, Mathf.Min(bodyHeight, availableHeight));
            if (bodyHeight > availableHeight) {
                Rect viewRect = new Rect(0f, 0f, bodyRect.width - 16f, bodyHeight);
                Widgets.BeginScrollView(bodyRect, ref scrollPos, viewRect);
                section.DrawBody(new Rect(0f, 0f, viewRect.width, bodyHeight), pawn, snap, ctx);
                Widgets.EndScrollView();
            } else {
                section.DrawBody(bodyRect, pawn, snap, ctx);
            }

            y += bodyRect.height;
        }
    }
}
