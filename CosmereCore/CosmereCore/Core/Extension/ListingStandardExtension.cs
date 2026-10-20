using System;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Extension;

public static class ListingStandardExtension {
    public static void Checkbox(
        this Listing_Standard listing,
        ref bool checkOn,
        string? tooltip = null,
        float height = 0.0f,
        float size = 24f,
        bool disabled = false
    ) {
        Rect rect = listing.GetRect(height != 0.0 ? height : size);
        rect.width = Math.Min(rect.width + 24f, listing.ColumnWidth);
        Rect? boundingRectCached = listing.BoundingRectCached;
        if (boundingRectCached.HasValue) {
            ref Rect local = ref rect;
            boundingRectCached = listing.BoundingRectCached;
            Rect other = boundingRectCached!.Value;
            if (!local.Overlaps(other)) {
                listing.Gap(listing.verticalSpacing);
                return;
            }
        }

        if (!tooltip.NullOrEmpty()) {
            if (Mouse.IsOver(rect)) {
                Widgets.DrawHighlight(rect);
            }

            TooltipHandler.TipRegion(rect, (TipSignal)tooltip);
        }

        Widgets.Checkbox(rect.x, rect.y, ref checkOn, size, disabled);
        listing.Gap(listing.verticalSpacing);
    }
}
