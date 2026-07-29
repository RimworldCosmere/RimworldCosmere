using Cosmere.Core.Settings.Model;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.Window;

public sealed class SettingsFooterRenderer {
    private const float ButtonHeight = 30f;
    private const float CloseButtonWidth = 112f;
    private const float ResetButtonWidth = 250f;
    private const float ButtonGap = 8f;
    private const float EdgePadding = 12f;

    private string? confirmationSystemKey;

    public void CancelConfirmation() {
        confirmationSystemKey = null;
    }

    public void Draw(
        Rect rect,
        string systemKey,
        string systemName,
        IReadOnlyList<SettingSection> sections,
        ISystemSkin skin,
        global::System.Action requestClose
    ) {
        Widgets.DrawBoxSolid(new Rect(rect.x, rect.y, rect.width, 1f), new Color(skin.BorderTintColor.r, skin.BorderTintColor.g, skin.BorderTintColor.b, 0.55f));

        if (confirmationSystemKey == systemKey) {
            DrawConfirmation(rect, systemName, sections, skin);
        } else {
            DrawReset(rect, systemKey, systemName, skin);
        }

        Rect closeRect = new Rect(rect.xMax - EdgePadding - CloseButtonWidth, ButtonY(rect), CloseButtonWidth, ButtonHeight);
        if (SettingsButtonRenderer.Draw(closeRect, (string)"CC_Settings_Close".Translate(), skin, SettingsButtonStyle.Primary)) {
            CancelConfirmation();
            requestClose();
        }
    }

    private void DrawReset(Rect rect, string systemKey, string systemName, ISystemSkin skin) {
        Rect resetRect = new Rect(rect.x + EdgePadding, ButtonY(rect), ResetButtonWidth, ButtonHeight);
        string label = (string)"CC_Settings_Reset_System".Translate(systemName.Named("SYSTEM"));
        if (SettingsButtonRenderer.Draw(resetRect, label, skin, SettingsButtonStyle.Destructive)) {
            confirmationSystemKey = systemKey;
        }
    }

    private void DrawConfirmation(Rect rect, string systemName, IReadOnlyList<SettingSection> sections, ISystemSkin skin) {
        string label = (string)"CC_Settings_Reset_Confirm".Translate(systemName.Named("SYSTEM"));
        float confirmWidth = 92f;
        float cancelWidth = 84f;
        Rect labelRect = new Rect(rect.x + EdgePadding, ButtonY(rect), rect.width - EdgePadding * 2f - confirmWidth - cancelWidth - ButtonGap * 2f - CloseButtonWidth - ButtonGap, ButtonHeight);
        UIText.EllipsisLabel(labelRect, label, GameFont.Small, TextAnchor.MiddleLeft, skin.HeaderTextColor);

        float buttonsX = labelRect.xMax + ButtonGap;
        Rect confirmRect = new Rect(buttonsX, ButtonY(rect), confirmWidth, ButtonHeight);
        if (SettingsButtonRenderer.Draw(confirmRect, (string)"CC_Settings_Confirm".Translate(), skin, SettingsButtonStyle.Destructive)) {
            ResetAll(sections);
            CancelConfirmation();
            return;
        }

        Rect cancelRect = new Rect(confirmRect.xMax + ButtonGap, ButtonY(rect), cancelWidth, ButtonHeight);
        if (SettingsButtonRenderer.Draw(cancelRect, (string)"CC_Settings_Cancel".Translate(), skin, SettingsButtonStyle.Neutral)) {
            CancelConfirmation();
        }
    }

    // The footer is taller than the row it holds, so the buttons ride its centre line
    // rather than hanging off the divider at the top.
    private static float ButtonY(Rect rect) {
        return rect.y + (rect.height - ButtonHeight) / 2f;
    }

    private static void ResetAll(IReadOnlyList<SettingSection> sections) {
        for (int sectionIndex = 0; sectionIndex < sections.Count; sectionIndex++) {
            IReadOnlyList<SettingDescriptor> settings = sections[sectionIndex].Settings;
            for (int settingIndex = 0; settingIndex < settings.Count; settingIndex++) {
                settings[settingIndex].Control.Reset();
            }
        }
    }
}
