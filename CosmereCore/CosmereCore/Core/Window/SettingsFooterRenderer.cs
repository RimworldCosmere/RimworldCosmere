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

    private static readonly Color DangerTint = new Color(1f, 0.62f, 0.58f);

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

        Rect closeRect = new Rect(rect.xMax - EdgePadding - CloseButtonWidth, rect.y + (rect.height - ButtonHeight) / 2f, CloseButtonWidth, ButtonHeight);
        if (DrawButton(closeRect, (string)"CC_Settings_Close".Translate(), false)) {
            CancelConfirmation();
            requestClose();
        }
    }

    private void DrawReset(Rect rect, string systemKey, string systemName, ISystemSkin skin) {
        Rect resetRect = new Rect(rect.x + EdgePadding, rect.y + (rect.height - ButtonHeight) / 2f, ResetButtonWidth, ButtonHeight);
        string label = (string)"CC_Settings_Reset_System".Translate(systemName.Named("SYSTEM"));
        if (DrawButton(resetRect, label, true)) {
            confirmationSystemKey = systemKey;
        }
    }

    private void DrawConfirmation(Rect rect, string systemName, IReadOnlyList<SettingSection> sections, ISystemSkin skin) {
        string label = (string)"CC_Settings_Reset_Confirm".Translate(systemName.Named("SYSTEM"));
        float confirmWidth = 92f;
        float cancelWidth = 84f;
        Rect labelRect = new Rect(rect.x + EdgePadding, rect.y, rect.width - EdgePadding * 2f - confirmWidth - cancelWidth - ButtonGap * 2f - CloseButtonWidth - ButtonGap, rect.height);
        UIText.EllipsisLabel(labelRect, label, GameFont.Small, TextAnchor.MiddleLeft, skin.HeaderTextColor);

        float buttonsX = labelRect.xMax + ButtonGap;
        Rect confirmRect = new Rect(buttonsX, rect.y + (rect.height - ButtonHeight) / 2f, confirmWidth, ButtonHeight);
        if (DrawButton(confirmRect, (string)"CC_Settings_Confirm".Translate(), true)) {
            ResetAll(sections);
            CancelConfirmation();
            return;
        }

        Rect cancelRect = new Rect(confirmRect.xMax + ButtonGap, rect.y + (rect.height - ButtonHeight) / 2f, cancelWidth, ButtonHeight);
        if (DrawButton(cancelRect, (string)"CC_Settings_Cancel".Translate(), false)) {
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

    // Widgets.ButtonText carries the vanilla button atlas, so these read as the same
    // buttons the rest of the game uses rather than flat boxes. Destructive actions tint
    // that atlas red instead of getting their own shape.
    private static bool DrawButton(Rect rect, string label, bool destructive) {
        Color previousColor = GUI.color;
        if (destructive) GUI.color = DangerTint;

        bool clicked;
        try {
            clicked = Widgets.ButtonText(rect, label);
        } finally {
            GUI.color = previousColor;
        }

        TooltipHandler.TipRegion(rect, label);

        return clicked;
    }
}
