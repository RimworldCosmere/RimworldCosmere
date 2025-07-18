using CosmereFramework.Listing;
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

    public override void DoTabContents(Rect inRect, ListingForm listing) {
        listing.Fieldset(
            "CS_Settings_Category_Incidents".Translate(),
            fieldset => {
                fieldset.Field(
                    "CS_Settings_MistsFrequency_Label".Translate(),
                    "CS_Settings_MistsFrequency_Tooltip".Translate(),
                    sub => UIUtil.IntEnumDropdown(sub, mistsFrequency, v => mistsFrequency = v, false)
                );
            },
            SubListingOptions.WithoutTopPadding()
        );

        listing.Fieldset(
            "CS_Settings_Category_Pawns".Translate(),
            fieldset => {
                fieldset.Field(
                    "CS_Settings_PawnsKeepVialOnDown_Label".Translate(),
                    "CS_Settings_PawnsKeepVialOnDown_Tooltip".Translate(),
                    sub => UIUtil.BoolEnumDropdown(sub, pawnsKeepVialsWhenDowned, v => pawnsKeepVialsWhenDowned = v)
                );

                fieldset.Field(
                    "CS_Settings_PawnsKeepMetalmindsOnDown_Label".Translate(),
                    "CS_Settings_PawnsKeepMetalmindsOnDown_Tooltip".Translate(),
                    sub => UIUtil.BoolEnumDropdown(
                        sub,
                        pawnsKeepMetalmindsWhenDowned,
                        v => pawnsKeepMetalmindsWhenDowned = v
                    )
                );
            }
        );
    }
}