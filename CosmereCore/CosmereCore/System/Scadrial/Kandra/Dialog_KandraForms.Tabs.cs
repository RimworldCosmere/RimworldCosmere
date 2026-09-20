using Cosmere.Core.UI;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>
///     The designer is three panes of one decision, not three windows. The strip picks the pane and
///     the preview plate above it never moves, so the thing being judged stays still while it flips.
/// </summary>
public partial class Dialog_KandraForms {
    private const float TabHeight = 32f;
    private const float TabStripGap = 2f;
    private const float TabUnderlineHeight = 2f;

    private DesignerTab tab = DesignerTab.Body;

    /// <summary>Draws the strip along the top of the region and hands back what is left below it.</summary>
    private Rect DrawTabStrip(Rect region) {
        Rect strip = region.TopPartPixels(TabHeight);
        float width = (strip.width - (TabStripGap * 2f)) / 3f;

        DrawTab(new Rect(strip.x, strip.y, width, TabHeight), DesignerTab.Body, "CS_Kandra_Tab_Body");

        DrawTab(
            new Rect(strip.x + width + TabStripGap, strip.y, width, TabHeight),
            DesignerTab.Hair,
            "CS_Kandra_Tab_Hair"
        );

        DrawTab(new Rect(strip.xMax - width, strip.y, width, TabHeight), DesignerTab.Eyes, "CS_Kandra_Tab_Eyes");

        float below = TabHeight + Spacing.Get(0.5f);

        return new Rect(region.x, strip.yMax + Spacing.Get(0.5f), region.width, region.height - below);
    }

    /// <summary>The live tab reads as the plate below it, carried by a rule in the window's own gold.</summary>
    private void DrawTab(Rect rect, DesignerTab which, string labelKey) {
        bool live = tab == which;

        Widgets.DrawMenuSection(rect);

        if (live) {
            Rect rule = new Rect(rect.x, rect.yMax - TabUnderlineHeight, rect.width, TabUnderlineHeight);
            Widgets.DrawBoxSolid(rule, BorderColor);
        } else {
            Widgets.DrawHighlightIfMouseover(rect);
        }

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, live ? BorderColor : bodyTextColor)) {
            Widgets.Label(rect, labelKey.Translate());
        }

        MouseoverSounds.DoRegion(rect);
        if (Widgets.ButtonInvisible(rect)) tab = which;
    }

    /// <summary>
    ///     Every tab is the same two columns: what the thing is on the left, what colour it is on the
    ///     right. The split is an argument because a shape grid needs room a colour list does not.
    /// </summary>
    private static (Rect left, Rect right) DrawColumnPlates(Rect region, float leftShare) {
        float gap = Spacing.Get();
        float left = (region.width - gap) * leftShare;

        Rect leftPlate = new Rect(region.x, region.y, left, region.height);
        Rect rightPlate = new Rect(leftPlate.xMax + gap, region.y, region.width - left - gap, region.height);

        Widgets.DrawMenuSection(leftPlate);
        Widgets.DrawMenuSection(rightPlate);

        return (leftPlate.ContractedBy(Spacing.Get(0.75f)), rightPlate.ContractedBy(Spacing.Get(0.75f)));
    }

    /// <summary>How wide a column's scroll content may be once the bar has taken its share.</summary>
    private static float ColumnContentWidth(Rect inner) {
        return inner.width - ScrollbarWidth;
    }

    /// <summary>The three panes the designer flips between. Body is what it opens on.</summary>
    private enum DesignerTab {
        Body,
        Hair,
        Eyes,
    }
}
