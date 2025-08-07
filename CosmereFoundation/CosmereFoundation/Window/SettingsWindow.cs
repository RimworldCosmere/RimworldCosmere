using Cosmere.Foundation.Listing;
using Cosmere.Foundation.Settings;
using Cosmere.Foundation.UI;
using UnityEngine;
using Verse;

namespace Cosmere.Foundation.Window;

public class SettingsWindow {
    private static readonly Padding ContentPadding = new Padding(16);
    private readonly List<CosmereModSettings> allModSettings;
    private readonly List<TabRecord> cachedTabs;

    private readonly Form listing = new Form { verticalSpacing = 6f, maxOneColumn = true };

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
        TabDrawer.DrawTabs(new Rect(inRect.xMin, inRect.yMin, inRect.width, TabDrawer.TabHeight), cachedTabs);

        listing.Contain(
            Box.Create(
                inRect.xMin,
                inRect.yMin,
                inRect.width,
                inRect.height - TabDrawer.TabHeight,
                ContentPadding,
                Texture2D.grayTexture
            ),
            selectedTab.DoTabContents
        );
    }
}