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
    public bool pawnsKeepMetalmindsWhenDowned;
    public bool pawnsKeepVialsWhenDowned;

    public override string Name => "Scadrial";

    public override void ExposeData() {
        Scribe_Values.Look(ref mistsFrequency, "mistsFrequency");
        Scribe_Values.Look(ref pawnsKeepMetalmindsWhenDowned, "pawnsKeepMetalmindsWhenDowned", true);
        Scribe_Values.Look(ref pawnsKeepVialsWhenDowned, "pawnsKeepVialsWhenDowned", true);
    }

    public override void DoTabContents(Rect inRect, Listing_Standard mainListing) {
        using (new TextBlock(GameFont.Medium)) mainListing.Label("CS_Settings_Category_Incidents".Translate());
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
                Widgets.Label(rect, "CS_Settings_MistsFrequency".Translate());

                sub.NewColumn();
                sub.ColumnWidth = Mathf.Min(100, width - SubListingLabelWidth - ListingColumnSpacing);
                UIUtil.IntEnumDropdown(sub, mistsFrequency, v => mistsFrequency = v, false);
            }
        );

        using (new TextBlock(GameFont.Small)) mainListing.Gap();
        using (new TextBlock(GameFont.Medium)) mainListing.Label("CS_Settings_Category_Pawns".Translate());
        using (new TextBlock(GameFont.Small)) {
            mainListing.GapLine();
            mainListing.Gap();
        }

        MakeSubListing(
            mainListing,
            expectedHeight,
            (sub, width) => {
                sub.ColumnWidth = SubListingLabelWidth;
                Rect rect = sub.GetRect(SubListingRowHeight);
                Widgets.Label(rect, "CS_Settings_PawnsKeepVialOnDown".Translate());

                sub.NewColumn();
                sub.ColumnWidth = Mathf.Min(100, width - SubListingLabelWidth - ListingColumnSpacing);
                UIUtil.BoolEnumDropdown(sub, pawnsKeepVialsWhenDowned, v => pawnsKeepVialsWhenDowned = v);
            }
        );
        MakeSubListing(
            mainListing,
            expectedHeight,
            (sub, width) => {
                sub.ColumnWidth = SubListingLabelWidth;
                Rect rect = sub.GetRect(SubListingRowHeight);
                Widgets.Label(rect, "CS_Settings_PawnsKeepMetalmindsOnDown".Translate());

                sub.NewColumn();
                sub.ColumnWidth = Mathf.Min(100, width - SubListingLabelWidth - ListingColumnSpacing);
                UIUtil.BoolEnumDropdown(sub, pawnsKeepMetalmindsWhenDowned, v => pawnsKeepMetalmindsWhenDowned = v);
            }
        );
    }
}