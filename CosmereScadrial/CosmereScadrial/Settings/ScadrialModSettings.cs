using CosmereFramework.Settings;
using CosmereFramework.Util;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Settings;

public enum MistsFrequency {
    Daily,
    Weekly,
    Monthly,
}

public class ScadrialModSettings : CosmereModSettings {
    public MistsFrequency mistsFrequency = MistsFrequency.Daily;

    public override string Name => "Scadrial";

    public override void ExposeData() {
        Scribe_Values.Look(ref mistsFrequency, "mistsFrequency");
    }

    public override void DoTabContents(Rect inRect, Listing_Standard mainListing) {
        using (new TextBlock(GameFont.Medium)) mainListing.Label("Incident Settings");
        using (new TextBlock(GameFont.Small)) {
            mainListing.GapLine();
            mainListing.Gap();
        }

        float expectedHeight = CategoryPadding * 2 + (SubListingRowHeight + SubListingSpacing) * 1;
        MakeSubListing(
            mainListing,
            expectedHeight,
            (sub, width) => {
                sub.ColumnWidth = SubListingLabelWidth;
                Rect rect = sub.GetRect(SubListingRowHeight);
                Widgets.Label(rect, "Mists Frequency:");

                sub.NewColumn();
                sub.ColumnWidth = Mathf.Min(100, width - SubListingLabelWidth - ListingColumnSpacing);
                UIUtil.IntEnumDropdown(sub, mistsFrequency, v => mistsFrequency = v, false);
            }
        );
    }
}