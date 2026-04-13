using Cosmere.Core.Listing;
using Cosmere.Core.Settings;
using Cosmere.Core.UI;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Window;

public class SettingsWindow {
    private static readonly Padding ContentPadding = new Padding(16);
    private readonly List<CosmereModSettings> allModSettings;
    private readonly List<TabRecord> cachedTabs;

    private readonly Form listing = new Form { verticalSpacing = 6f, maxOneColumn = true };

    private CosmereModSettings selectedTab;

    public SettingsWindow(List<CosmereModSettings> allModSettings) {
        this.allModSettings = allModSettings;

        // Ensure Core tab is first
        CosmereModSettings? coreSettings = allModSettings.FirstOrDefault(s => s.Name == "Core");
        selectedTab = coreSettings ?? allModSettings.First();

        cachedTabs = this.allModSettings
            .OrderBy(s => s.Name == "Core" ? 0 : 1)
            .ThenBy(s => s.Name)
            .Select(modSettings => new TabRecord(
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