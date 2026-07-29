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
        Widgets.DrawBoxSolid(rect, new Color(0.05f, 0.06f, 0.08f, 0.92f));
        Widgets.DrawBoxSolid(new Rect(rect.x, rect.y, rect.width, 1f), new Color(skin.BorderTintColor.r, skin.BorderTintColor.g, skin.BorderTintColor.b, 0.55f));

        if (confirmationSystemKey == systemKey) {
            DrawConfirmation(rect, systemName, sections, skin);
        } else {
            DrawReset(rect, systemKey, systemName, skin);
        }

        Rect closeRect = new Rect(rect.xMax - CloseButtonWidth, rect.y + (rect.height - ButtonHeight) / 2f, CloseButtonWidth, ButtonHeight);
        if (DrawButton(closeRect, (string)"CC_Settings_Close".Translate(), skin.AccentColor, false)) {
            CancelConfirmation();
            requestClose();
        }
    }

    private void DrawReset(Rect rect, string systemKey, string systemName, ISystemSkin skin) {
        Rect resetRect = new Rect(rect.x, rect.y + (rect.height - ButtonHeight) / 2f, ResetButtonWidth, ButtonHeight);
        string label = (string)"CC_Settings_Reset_System".Translate(systemName.Named("SYSTEM"));
        if (DrawButton(resetRect, label, skin.AccentColor, true)) {
            confirmationSystemKey = systemKey;
        }
    }

    private void DrawConfirmation(Rect rect, string systemName, IReadOnlyList<SettingSection> sections, ISystemSkin skin) {
        string label = (string)"CC_Settings_Reset_Confirm".Translate(systemName.Named("SYSTEM"));
        float confirmWidth = 92f;
        float cancelWidth = 84f;
        Rect labelRect = new Rect(rect.x, rect.y, rect.width - confirmWidth - cancelWidth - ButtonGap * 2f - CloseButtonWidth - ButtonGap, rect.height);
        UIText.EllipsisLabel(labelRect, label, GameFont.Small, TextAnchor.MiddleLeft, skin.HeaderTextColor);

        float buttonsX = labelRect.xMax + ButtonGap;
        Rect confirmRect = new Rect(buttonsX, rect.y + (rect.height - ButtonHeight) / 2f, confirmWidth, ButtonHeight);
        if (DrawButton(confirmRect, (string)"CC_Settings_Confirm".Translate(), skin.AccentColor, true)) {
            ResetAll(sections);
            CancelConfirmation();
            return;
        }

        Rect cancelRect = new Rect(confirmRect.xMax + ButtonGap, rect.y + (rect.height - ButtonHeight) / 2f, cancelWidth, ButtonHeight);
        if (DrawButton(cancelRect, (string)"CC_Settings_Cancel".Translate(), skin.AccentColor, false)) {
            CancelConfirmation();
        }
    }

    private static void ResetAll(IReadOnlyList<SettingSection> sections) {
        for (int sectionIndex = 0; sectionIndex < sections.Count; sectionIndex++) {
            IReadOnlyList<SettingDescriptor> settings = sections[sectionIndex].Settings;
            for (int settingIndex = 0; settingIndex < settings.Count; settingIndex++) {
                settings[settingIndex].Control.Reset();
            }
        }
    }

    private static bool DrawButton(Rect rect, string label, Color accent, bool primary) {
        Color fill = primary
            ? new Color(accent.r, accent.g, accent.b, 0.22f)
            : new Color(1f, 1f, 1f, 0.08f);
        Color textColor = primary ? Color.white : new Color(0.82f, 0.84f, 0.88f);
        Widgets.DrawBoxSolid(rect, fill);
        if (primary) {
            Widgets.DrawBoxSolid(new Rect(rect.x, rect.yMax - 2f, rect.width, 2f), accent);
        }

        UIText.EllipsisLabel(rect.ContractedBy(6f, 0f), label, GameFont.Small, TextAnchor.MiddleCenter, textColor);
        TooltipHandler.TipRegion(rect, label);
        Widgets.DrawHighlightIfMouseover(rect);
        MouseoverSounds.DoRegion(rect);
        return Widgets.ButtonInvisible(rect);
    }
}
