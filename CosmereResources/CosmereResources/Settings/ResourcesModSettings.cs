using Cosmere.Framework.Listing;
using Cosmere.Framework.Settings;
using UnityEngine;

namespace Cosmere.Resources.Settings;

public class ResourcesModSettings : CosmereModSettings {
    public override bool Enabled => false;
    public override string Name => "Resources";

    public override void DoTabContents(Rect inRect, ListingForm listing) {
        listing.Label("There are no settings for this!");
    }
}