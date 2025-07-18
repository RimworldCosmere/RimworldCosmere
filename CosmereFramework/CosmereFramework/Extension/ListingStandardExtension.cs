using System;
using UnityEngine;
using Verse;

namespace CosmereFramework.Extension;

public record FormFieldOptions {
    public float columnSpacing = ListingStandardExtension.ColumnSpacing;
    public float height = ListingStandardExtension.SubListingRowHeight;
    public float labelWidth = ListingStandardExtension.SubListingLabelWidth;
    public float minimumColumnWidth = 100;
}

public static class ListingStandardExtension {
    public const float SubListingPadding = 5f;
    public const float SubListingInset = 30f;
    public const float SubListingVerticalSpacing = 2f;
    public const float SubListingRowHeight = 40f;
    public const float SubListingLabelWidth = 150f;
    public const float ColumnSpacing = 17f;
    public const float ExpectedHeight = SubListingPadding * 2 + (SubListingRowHeight + SubListingVerticalSpacing) * 1;

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
            Rect other = boundingRectCached.Value;
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