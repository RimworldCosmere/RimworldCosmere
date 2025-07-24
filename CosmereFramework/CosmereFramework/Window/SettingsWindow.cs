using Cosmere.Framework.Listing;
using Cosmere.Framework.Settings;
using Cosmere.Framework.UI;
using UnityEngine;
using Verse;

namespace Cosmere.Framework.Window;

public class SettingsWindow {
    private static readonly float TabHeight = 55f;
    private static readonly Padding TabPadding = new Padding(8f, 10f);
    private static readonly Padding ContentPadding = new Padding(16f);

    private readonly List<CosmereModSettings> allModSettings;
    private readonly List<TabRecord> cachedTabs;

    private readonly ListingForm listing = new ListingForm { verticalSpacing = 6f };

    private CosmereModSettings selectedTab;

    public SettingsWindow(List<CosmereModSettings> allModSettings) {
        this.allModSettings = allModSettings;
        selectedTab = allModSettings.First();

        cachedTabs = this.allModSettings.Select(modSettings => new TabRecord(
                    modSettings.Name,
                    delegate { selectedTab = modSettings; },
                    () => selectedTab == modSettings
                )
            )
            .ToList();
    }

    public void DoWindowContents(Rect inRect) {
        Rect rect = new Rect(0, inRect.yMin + 40, inRect.width, TabDrawer.TabHeight);
        TabDrawer.DrawTabs(rect, cachedTabs);

        Rect listingRect = Box.Create(
            inRect.xMin,
            inRect.yMin + 40,
            inRect.width,
            inRect.height - TabHeight - ContentPadding.y * 2,
            ContentPadding,
            Texture2D.grayTexture
        );

        listing.Contain(listingRect, selectedTab.DoTabContents);
    }
}