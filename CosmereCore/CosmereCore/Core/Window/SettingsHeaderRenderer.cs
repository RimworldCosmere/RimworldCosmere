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
    private const float CloseSize = 24f;

    // Vanilla hangs its own close X off the window chrome. This one lives in our top bar
    // instead, so the header owns the whole row rather than leaving a gap for it.
    public static bool DrawCrest(Rect rect, ISystemSkin skin) {
        Widgets.DrawBoxSolid(new Rect(rect.x, rect.yMax - 2f, rect.width, 2f), skin.AccentColor);

        Rect closeRect = new Rect(
            rect.xMax - CrestPadding - CloseSize,
            rect.y + (rect.height - CloseSize) / 2f,
            CloseSize,
            CloseSize
        );
        bool closeRequested = DrawClose(closeRect, skin);

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
            new Rect(textX, rect.y, closeRect.x - textX - CrestPadding, rect.height),
            skin.HeaderLabel,
            skin.HeaderFont,
            TextAnchor.MiddleLeft,
            skin.HeaderTextColor
        );

        return closeRequested;
    }

    public static string? DrawSectionTabs(
        Rect rect,
        ISystemSkin skin,
        IReadOnlyList<SettingSection> sections,
        string? selectedSectionKey
    ) {
        List<SettingSection> visibleSections = [];
        for (int i = 0; i < sections.Count; i++) {
            SettingSection section = sections[i];
            try {
                if (section.IsVisible) visibleSections.Add(section);
            } catch (global::System.Exception exception) {
                Logger.Error($"Settings section tab {section.Key} failed: {exception}");
            }
        }

        if (visibleSections.Count == 0) return null;

        string? requested = null;
        float sectionWidth = rect.width / visibleSections.Count;
        for (int i = 0; i < visibleSections.Count; i++) {
            SettingSection section = visibleSections[i];
            Rect sectionRect = new Rect(rect.x + sectionWidth * i, rect.y, sectionWidth, rect.height);
            bool selected = section.Key == selectedSectionKey;
            if (selected) {
                Widgets.DrawBoxSolid(sectionRect, new Color(1f, 1f, 1f, 0.06f));
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
            if (Widgets.ButtonInvisible(sectionRect)) requested = section.Key;
        }

        return requested;
    }

    private static bool DrawClose(Rect rect, ISystemSkin skin) {
        UIText.EllipsisLabel(rect, "×", GameFont.Medium, TextAnchor.MiddleCenter, skin.HeaderTextColor);
        TooltipHandler.TipRegion(rect, "CloseButton".Translate());
        Widgets.DrawHighlightIfMouseover(rect);
        MouseoverSounds.DoRegion(rect);

        return Widgets.ButtonInvisible(rect);
    }
}
