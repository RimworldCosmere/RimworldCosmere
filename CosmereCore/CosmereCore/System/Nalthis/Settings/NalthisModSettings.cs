using Cosmere.Core.Listing;
using Cosmere.Core.Settings;
using Verse;

namespace Cosmere.System.Nalthis.Settings;

public class NalthisModSettings : CosmereModSettings {
    public bool enableAwakening = true;

    public override string Name => "Nalthis";

    public override void ExposeData() {
        Scribe_Values.Look(ref enableAwakening, "enableAwakening", true);
    }

    public override void DoTabContents(Form listing) {
        listing.Fieldset(
            "CN_Settings_Category_Awakening".Translate(),
            fieldset => {
                fieldset.Field(
                    "CN_Settings_AwakeningEnabled_Label".Translate(),
                    "CN_Settings_AwakeningEnabled_Tooltip".Translate(),
                    sub => sub.Checkbox(ref enableAwakening)
                );
            },
            SubListingOptions.WithoutTopPadding()
        );
    }
}
