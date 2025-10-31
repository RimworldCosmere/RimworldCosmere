using Cosmere.Core.Listing;
using Verse;

namespace Cosmere.Core.Settings;

public class CoreModSettings : CosmereModSettings {
    public bool showDormantConnection;

    public override string Name => "Core";

    public override void DoTabContents(Form listing) {
        listing.Fieldset(
            "CC_Settings_Category_Connection".Translate(),
            fieldset => {
                fieldset.Field(
                    "CC_Settings_Connection_ShowDormantConnection_Label".Translate(),
                    "CC_Settings_Connection_ShowDormantConnection_Tooltip".Translate(),
                    sub => sub.Checkbox(ref showDormantConnection)
                );
            },
            SubListingOptions.WithoutTopPadding()
        );
    }

    public override void ExposeData() {
        Scribe_Values.Look(ref showDormantConnection, "showDormantConnection");
    }
}