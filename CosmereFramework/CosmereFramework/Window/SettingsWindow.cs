using System.Collections.Generic;
using System.Linq;
using CosmereFramework.Settings;
using CosmereFramework.UI;
using LudeonTK;
using UnityEngine;
using Verse;

namespace CosmereFramework.Window;

public class SettingsWindow {
    private static readonly float TabHeight = 55f;
    private static readonly Padding TabPadding = new Padding(8f, 10f);
    private static readonly Padding ContentPadding = new Padding(16f);

    private readonly List<CosmereModSettings> allModSettings;

    private readonly Listing_Standard mainListing = new Listing_Standard { verticalSpacing = 6f };

    private CosmereModSettings selectedTab;

    public SettingsWindow(List<CosmereModSettings> allModSettings) {
        this.allModSettings = allModSettings;
        selectedTab = allModSettings.First();
    }

    public void DoWindowContents(Rect inRect) {
        float currentX = inRect.xMin;

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter)) {
            foreach (CosmereModSettings modSettings in allModSettings) {
                Vector2 size = Text.CalcSize(modSettings.Name);
                float width = size.x + TabPadding.x * 2f;
                Rect tabRect = new Rect(currentX, inRect.yMin, width, TabHeight);
                bool isSelected = selectedTab == modSettings;

                Widgets.DrawBox(tabRect, 1, Texture2D.grayTexture);
                if (Widgets.ButtonInvisible(tabRect)) {
                    selectedTab = modSettings;
                }

                using (new TextBlock(Mouse.IsOver(tabRect) || isSelected ? Color.yellow : Color.white)) {
                    Widgets.Label(tabRect, modSettings.Name);
                }

                currentX += width + TabPadding.y;
            }
        }

        Rect listingRect = Box.Create(
            inRect.xMin,
            inRect.yMin + TabHeight,
            inRect.width,
            inRect.height - TabHeight - ContentPadding.y * 2,
            ContentPadding,
            Texture2D.grayTexture
        );

        mainListing.Begin(listingRect);
        selectedTab.DoTabContents(listingRect, mainListing);
        mainListing.End();
    }
}