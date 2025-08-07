using Cosmere.Foundation.Listing;
using Cosmere.Foundation.Settings;

namespace Cosmere.Resources.Settings;

public class ResourcesModSettings : CosmereModSettings {
    public override bool Enabled => false;
    public override string Name => "Resources";

    public override void DoTabContents(Form listing) {
        listing.Label("There are no settings for this!");
    }
}