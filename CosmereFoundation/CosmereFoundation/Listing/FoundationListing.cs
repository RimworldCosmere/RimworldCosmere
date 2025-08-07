using Cosmere.Foundation.UI;
using UnityEngine;
using Verse;

namespace Cosmere.Foundation.Listing;

public class FoundationListing : Listing_Standard {
    public float CurrentY => curY;
    public float CurrentX => curX;
    public Rect ListingRect => listingRect;

    public new void Gap(float? gapHeight = null) {
        curY += gapHeight ?? Spacing.Get();
    }

    public new void GapLine(float? gapHeight = null, Color? color = null) {
        gapHeight ??= Spacing.Get();
        color ??= new Color(1f, 1f, 1f, 0.4f);

        float y = curY + gapHeight.Value / 2f;
        using (new TextBlock(GUI.color * color.Value)) {
            Widgets.DrawLineHorizontal(curX, y, ColumnWidth);
        }

        curY += gapHeight.Value;
    }
}