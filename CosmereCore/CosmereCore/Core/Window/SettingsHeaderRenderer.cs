using Cosmere.Core.Settings.Model;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.Window;

public static class SettingsHeaderRenderer {
    private const float CrestPadding = 10f;
    private const float SigilSize = 26f;

    public static void DrawCrest(Rect rect, ISystemSkin skin) {
        Widgets.DrawBoxSolid(rect, new Color(skin.PanelBackgroundColor.r, skin.PanelBackgroundColor.g, skin.PanelBackgroundColor.b, 0.42f));
        Widgets.DrawBoxSolid(new Rect(rect.x, rect.yMax - 2f, rect.width, 2f), skin.AccentColor);

        float textX = rect.x + CrestPadding;
        if (skin.Sigil != null) {
            Rect sigilRect = new Rect(
                rect.x + CrestPadding,
                rect.y + (rect.height - SigilSize) / 2f,
                SigilSize,
                SigilSize
            );
            Color previousColor = GUI.color;
            GUI.color = skin.AccentColor;
            GUI.DrawTexture(sigilRect, skin.Sigil);
            GUI.color = previousColor;
            textX = sigilRect.xMax + 8f;
        }

        UIText.EllipsisLabel(
            new Rect(textX, rect.y, rect.xMax - textX - CrestPadding, rect.height),
            skin.HeaderLabel,
            skin.HeaderFont,
            TextAnchor.MiddleLeft,
            skin.HeaderTextColor
        );
    }

    public static string? DrawSectionRail(
        Rect rect,
        ISystemSkin skin,
        IReadOnlyList<SettingSection> sections,
        string? selectedSectionKey
    ) {
        Widgets.DrawBoxSolid(rect, new Color(skin.PanelBackgroundColor.r, skin.PanelBackgroundColor.g, skin.PanelBackgroundColor.b, 0.3f));

        int visibleCount = 0;
        for (int i = 0; i < sections.Count; i++) {
            if (sections[i].IsVisible) visibleCount++;
        }

        if (visibleCount == 0) return null;

        float sectionWidth = rect.width / visibleCount;
        int visibleIndex = 0;
        for (int i = 0; i < sections.Count; i++) {
            SettingSection section = sections[i];
            if (!section.IsVisible) continue;

            Rect sectionRect = new Rect(rect.x + sectionWidth * visibleIndex, rect.y, sectionWidth, rect.height);
            bool selected = section.Key == selectedSectionKey;
            if (selected) {
                Widgets.DrawBoxSolid(sectionRect, new Color(skin.AccentColor.r, skin.AccentColor.g, skin.AccentColor.b, 0.12f));
                Widgets.DrawBoxSolid(new Rect(sectionRect.x + 4f, sectionRect.yMax - 2f, sectionRect.width - 8f, 2f), skin.AccentColor);
            }

            string label = (string)section.TitleKey.Translate();
            UIText.EllipsisLabel(
                sectionRect.ContractedBy(6f, 0f),
                label,
                GameFont.Small,
                TextAnchor.MiddleCenter,
                selected ? skin.HeaderTextColor : new Color(0.70f, 0.72f, 0.76f)
            );
            TooltipHandler.TipRegion(sectionRect, label);
            Widgets.DrawHighlightIfMouseover(sectionRect);
            MouseoverSounds.DoRegion(sectionRect);
            if (Widgets.ButtonInvisible(sectionRect)) return section.Key;

            visibleIndex++;
        }

        return null;
    }
}
