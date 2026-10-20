using Cosmere.Core.Settings;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.Window;

public static class SettingsSidebarRenderer {
    private const float SearchHeight = 32f;
    private const float RowHeight = 44f;
    private const float RowGap = 4f;
    private const float Padding = 8f;
    private const float SigilSize = 24f;

    public static CosmereModSettings? Draw(
        Rect rect,
        IReadOnlyList<CosmereModSettings> settings,
        CosmereModSettings selectedSettings,
        ref string searchText
    ) {
        // divider rides the middle of the gutter, not the sidebar's own edge, so both sides get equal air
        Widgets.DrawBoxSolid(
            new Rect(rect.xMax + SettingsWindowLayout.SidebarGutter / 2f, rect.y, 1f, rect.height),
            new Color(1f, 1f, 1f, 0.09f)
        );

        // Vanilla's own dialog title is suppressed, so the window names itself here.
        Rect titleRect = new Rect(rect.x + Padding, rect.y, rect.width - Padding * 2f, SettingsWindowLayout.TitleHeight);
        UIText.EllipsisLabel(
            titleRect,
            (string)"CC_Settings_Title".Translate(),
            GameFont.Medium,
            TextAnchor.MiddleLeft,
            new Color(0.88f, 0.90f, 0.93f)
        );

        Rect searchRect = new Rect(rect.x + Padding, titleRect.yMax, rect.width - Padding * 2f, SearchHeight);
        searchText = Widgets.TextField(searchRect, searchText);
        if (searchText.NullOrEmpty()) {
            UIText.EllipsisLabel(
                searchRect.ContractedBy(6f, 0f),
                (string)"CC_Settings_Search_Placeholder".Translate(),
                GameFont.Small,
                TextAnchor.MiddleLeft,
                new Color(0.62f, 0.64f, 0.68f)
            );
        }

        TooltipHandler.TipRegion(searchRect, "CC_Settings_Search_Placeholder".Translate());
        Widgets.DrawHighlightIfMouseover(searchRect);
        MouseoverSounds.DoRegion(searchRect);

        float y = searchRect.yMax + Padding;
        for (int i = 0; i < settings.Count; i++) {
            CosmereModSettings systemSettings = settings[i];
            ISystemSkin skin = SystemSkinRegistry.ForOrFallback(systemSettings.SkinId);
            Rect row = new Rect(rect.x + Padding, y, rect.width - Padding * 2f, RowHeight);
            bool selected = systemSettings == selectedSettings;

            DrawSystemRow(row, systemSettings.DisplayLabel, skin, selected);
            TooltipHandler.TipRegion(row, systemSettings.DisplayLabel);
            Widgets.DrawHighlightIfMouseover(row);
            MouseoverSounds.DoRegion(row);
            if (Widgets.ButtonInvisible(row)) return systemSettings;

            y += RowHeight + RowGap;
        }

        return null;
    }

    private static void DrawSystemRow(Rect rect, string label, ISystemSkin skin, bool selected) {
        if (selected) {
            Widgets.DrawBoxSolid(rect, new Color(skin.AccentColor.r, skin.AccentColor.g, skin.AccentColor.b, 0.16f));
            Widgets.DrawBoxSolid(new Rect(rect.x, rect.y, 3f, rect.height), skin.AccentColor);
        } else {
            Widgets.DrawBoxSolid(rect, new Color(1f, 1f, 1f, 0.025f));
        }

        Rect sigilRect = new Rect(rect.x + 8f, rect.y + (rect.height - SigilSize) / 2f, SigilSize, SigilSize);
        if (skin.Sigil != null) {
            Color previousColor = GUI.color;
            GUI.color = selected ? skin.AccentColor : new Color(0.58f, 0.61f, 0.65f);
            GUI.DrawTexture(sigilRect, skin.Sigil);
            GUI.color = previousColor;
        }

        Color labelColor = selected ? skin.HeaderTextColor : new Color(0.72f, 0.74f, 0.78f);
        UIText.EllipsisLabel(
            new Rect(sigilRect.xMax + 8f, rect.y, rect.xMax - sigilRect.xMax - 16f, rect.height),
            label,
            GameFont.Small,
            TextAnchor.MiddleLeft,
            labelColor
        );
    }
}
