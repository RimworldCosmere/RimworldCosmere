using System;
using CosmereFramework.Extension;
using CosmereFramework.UI;
using UnityEngine;
using Verse;

namespace CosmereFramework.Settings;

public abstract class CosmereModSettings : IExposable {
    protected const float CategoryPadding = 5f;
    protected const float CategoryInset = 30f;
    protected const float SubListingSpacing = 2f;
    protected const float SubListingLabelWidth = 150f;
    protected const float SubListingRowHeight = 40f;
    protected const float ListingColumnSpacing = 17f;

    public abstract string Name { get; }

    public abstract void ExposeData();

    public abstract void DoTabContents(Rect inRect, Listing_Standard listing);

    protected void MakeSubListing(
        Listing_Standard mainListing,
        float allocatedHeight,
        Action<Listing_Standard, float> drawContents,
        float padding = CategoryPadding,
        float extraInset = CategoryInset,
        float verticalSpacing = SubListingSpacing,
        TextAnchor anchor = TextAnchor.UpperLeft
    ) {
        using (new TextBlock(anchor)) {
            Rect subRect = mainListing.GetRect(allocatedHeight)
                .ContractedBy(new Padding(padding, padding, padding, padding + extraInset));
            Listing_Standard sub = new Listing_Standard { verticalSpacing = verticalSpacing };
            sub.Begin(subRect);
            drawContents(sub, subRect.width);
            sub.End();
        }
    }
}