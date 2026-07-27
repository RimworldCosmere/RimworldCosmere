using Cosmere.Core.Listing;
using Cosmere.Core.Settings;
using Verse;
using CoreUI = Cosmere.Core.UI.UIHelpers;

namespace Cosmere.System.Scadrial.Settings;

public enum MistsFrequency {
    Daily,
    Weekly,
    Monthly,
}

public class ScadrialModSettings : CosmereModSettings {
    public bool alwaysShowAllomanticAuras;
    public bool enableMists = true;
    public MistsFrequency mistsFrequency = MistsFrequency.Daily;
    public bool pawnsKeepMetalmindsWhenDowned;
    public bool pawnsKeepVialsWhenDowned;

    public override string Name => "Scadrial";

    public override void ExposeData() {
        Scribe_Values.Look(ref enableMists, "enableMists", true);
        Scribe_Values.Look(ref mistsFrequency, "mistsFrequency");
        Scribe_Values.Look(ref pawnsKeepMetalmindsWhenDowned, "pawnsKeepMetalmindsWhenDowned", true);
        Scribe_Values.Look(ref pawnsKeepVialsWhenDowned, "pawnsKeepVialsWhenDowned", true);
        Scribe_Values.Look(ref alwaysShowAllomanticAuras, "alwaysShowAllomanticAuras");
    }

    public override void DoTabContents(Form listing) {
        listing.Fieldset(
            "CS_Settings_Category_Incidents".Translate(),
            fieldset => {
                fieldset.Field(
                    "CS_Settings_MistsEnabled_Label".Translate(),
                    "CS_Settings_MistsEnabled_Tooltip".Translate(),
                    sub => sub.Checkbox(ref enableMists)
                );

                fieldset.Field(
                    "CS_Settings_MistsFrequency_Label".Translate(),
                    "CS_Settings_MistsFrequency_Tooltip".Translate(),
                    sub => CoreUI.IntEnumDropdown(sub, mistsFrequency, v => mistsFrequency = v, false)
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
                    sub => CoreUI.BoolEnumDropdown(sub, pawnsKeepVialsWhenDowned, v => pawnsKeepVialsWhenDowned = v)
                );

                fieldset.Field(
                    "CS_Settings_PawnsKeepMetalmindsOnDown_Label".Translate(),
                    "CS_Settings_PawnsKeepMetalmindsOnDown_Tooltip".Translate(),
                    sub => CoreUI.BoolEnumDropdown(
                        sub,
                        pawnsKeepMetalmindsWhenDowned,
                        v => pawnsKeepMetalmindsWhenDowned = v
                    )
                );
            }
        );

        listing.Fieldset(
            "CS_Settings_Category_Allomancy".Translate(),
            fieldset => {
                fieldset.Field(
                    "CS_Settings_AlwaysShowAuras_Label".Translate(),
                    "CS_Settings_AlwaysShowAuras_Tooltip".Translate(),
                    sub => sub.Checkbox(ref alwaysShowAllomanticAuras)
                );
            }
        );
    }
}
