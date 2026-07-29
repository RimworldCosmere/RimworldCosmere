using System;
using Cosmere.Core.Settings.Layout;
using Cosmere.Core.Settings.Model;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Dock;
using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Window;

public sealed class SettingsContentRenderer {
    private const float ColumnGap = SettingsWindowLayout.Gap;
    private const float SectionPadding = 12f;
    private const float SectionTitleHeight = 18f;
    private const float DividerHeight = 1f;
    private const float DividerGap = 8f;
    private const float RowGap = 8f;
    private const float RowVerticalPadding = 6f;
    private const float LabelWidthRatio = 0.48f;

    private readonly SettingsControlRenderer controlRenderer = new SettingsControlRenderer();
    private readonly List<MeasuredSection> measuredSections = [];
    private readonly List<SettingSectionMeasurement> sectionMeasurements = [];
    private readonly Dictionary<string, float> sectionOffsets = [];
    private readonly Dictionary<string, float> rowOffsets = [];

    public float ContentHeight { get; private set; }

    public IReadOnlyDictionary<string, float> SectionOffsets => sectionOffsets;

    public IReadOnlyDictionary<string, float> RowOffsets => rowOffsets;

    public void Measure(string systemKey, IReadOnlyList<SettingSection> sections, float contentWidth) {
        measuredSections.Clear();
        sectionMeasurements.Clear();
        sectionOffsets.Clear();
        rowOffsets.Clear();

        float columnWidth = GetColumnWidth(contentWidth);
        for (int i = 0; i < sections.Count; i++) {
            SettingSection section = sections[i];
            try {
                if (!section.IsVisible) continue;

                MeasuredSection measured = MeasureSection(systemKey, section, columnWidth);
                measuredSections.Add(measured);
                sectionMeasurements.Add(new SettingSectionMeasurement(section.Key, measured.Height));
            } catch (Exception exception) {
                Logger.Error($"Settings section {systemKey}/{section.Key} failed: {exception}");
            }
        }

        SettingsColumnLayoutResult layout = SettingsColumnLayout.Place(sectionMeasurements, columnWidth, ColumnGap);
        ContentHeight = layout.ContentHeight + SettingsWindowLayout.ContentPadding * 2f;
        AssignPlacements(layout.Placements);
    }

    public void Draw(
        Rect contentRect,
        Rect viewportRect,
        string systemKey,
        ISystemSkin skin,
        SettingsNavigationState navigation,
        bool reduceMotion
    ) {
        Widgets.DrawBoxSolid(
            contentRect,
            new Color(skin.PanelBackgroundColor.r, skin.PanelBackgroundColor.g, skin.PanelBackgroundColor.b, 0.30f)
        );

        if (measuredSections.Count == 0) {
            UIText.EllipsisLabel(
                contentRect.ContractedBy(SettingsWindowLayout.ContentPadding),
                (string)"CC_Settings_System_Empty".Translate(),
                GameFont.Small,
                TextAnchor.MiddleCenter,
                skin.HeaderTextColor
            );
            return;
        }

        SettingsNavigationState.FlashState flash = default;
        bool hasFlash = navigation.TryGetFlash(reduceMotion, out flash, out float flashAlpha) && flash.SystemKey == systemKey;
        Rect visibleRect = new Rect(0f, viewportRect.y, contentRect.width, viewportRect.height);

        for (int i = 0; i < measuredSections.Count; i++) {
            MeasuredSection section = measuredSections[i];
            if (!section.Rect.Overlaps(visibleRect)) continue;

            DrawSection(section, systemKey, skin, flash, hasFlash, flashAlpha);
        }
    }

    public bool TryGetSectionOffset(string sectionKey, out float offset) {
        return sectionOffsets.TryGetValue(sectionKey, out offset);
    }

    private MeasuredSection MeasureSection(string systemKey, SettingSection section, float columnWidth) {
        List<MeasuredRow> rows = [];
        float labelWidth = GetLabelWidth(columnWidth);
        float height = SectionPadding + SectionTitleHeight + DividerGap + DividerHeight + DividerGap;

        for (int i = 0; i < section.Settings.Count; i++) {
            SettingDescriptor descriptor = section.Settings[i];
            try {
                if (!descriptor.IsVisible) continue;

                float rowHeight = MeasureRow(descriptor, labelWidth);
                rows.Add(new MeasuredRow(descriptor, rowHeight));
                height += rowHeight;
                if (rows.Count > 1) height += RowGap;
            } catch (Exception exception) {
                Logger.Error($"Settings descriptor {systemKey}/{section.Key}/{descriptor.Key} failed: {exception}");
                rows.Add(new MeasuredRow(descriptor, controlRenderer.HeightFor(descriptor.Control) + RowVerticalPadding * 2f, true));
                height += rows[^1].Height;
                if (rows.Count > 1) height += RowGap;
            }
        }

        return new MeasuredSection(section, rows, height + SectionPadding);
    }

    private float MeasureRow(SettingDescriptor descriptor, float labelWidth) {
        float labelHeight;
        using (new TextBlock(GameFont.Small)) {
            labelHeight = Text.CalcHeight((string)descriptor.LabelKey.Translate(), labelWidth);
        }

        float descriptionHeight = 0f;
        if (!descriptor.DescriptionKey.NullOrEmpty()) {
            using (new TextBlock(GameFont.Tiny)) {
                descriptionHeight = Text.CalcHeight((string)descriptor.DescriptionKey.Translate(), labelWidth);
            }
        }

        return Mathf.Max(controlRenderer.HeightFor(descriptor.Control), labelHeight + descriptionHeight) + RowVerticalPadding * 2f;
    }

    private void AssignPlacements(IReadOnlyList<SettingSectionPlacement> placements) {
        for (int i = 0; i < placements.Count; i++) {
            SettingSectionPlacement placement = placements[i];
            MeasuredSection? section = FindSection(placement.Key);
            if (section == null) continue;

            section.Rect = new Rect(
                SettingsWindowLayout.ContentPadding + placement.X,
                SettingsWindowLayout.ContentPadding + placement.Y,
                placement.Width,
                placement.Height
            );
            sectionOffsets[section.Section.Key] = section.Rect.y;

            float rowY = section.Rect.y + SectionPadding + SectionTitleHeight + DividerGap + DividerHeight + DividerGap;
            for (int rowIndex = 0; rowIndex < section.Rows.Count; rowIndex++) {
                MeasuredRow row = section.Rows[rowIndex];
                row.Rect = new Rect(
                    section.Rect.x + SectionPadding,
                    rowY,
                    section.Rect.width - SectionPadding * 2f,
                    row.Height
                );
                rowOffsets[section.Section.Key + "/" + row.Descriptor.Key] = row.Rect.y;
                rowY += row.Height + RowGap;
            }
        }
    }

    private void DrawSection(
        MeasuredSection section,
        string systemKey,
        ISystemSkin skin,
        SettingsNavigationState.FlashState flash,
        bool hasFlash,
        float flashAlpha
    ) {
        Color fill = new Color(skin.PanelBackgroundColor.r, skin.PanelBackgroundColor.g, skin.PanelBackgroundColor.b, 0.76f);
        Color border = new Color(skin.BorderTintColor.r, skin.BorderTintColor.g, skin.BorderTintColor.b, 0.55f);
        Panel.Draw(section.Rect, fill, border);

        Rect titleRect = new Rect(
            section.Rect.x + SectionPadding,
            section.Rect.y + SectionPadding,
            section.Rect.width - SectionPadding * 2f,
            SectionTitleHeight
        );
        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleLeft, skin.HeaderTextColor)) {
            Widgets.Label(titleRect, ((string)section.Section.TitleKey.Translate()).ToUpperInvariant());
        }

        Rect dividerRect = new Rect(titleRect.x, titleRect.yMax + DividerGap, titleRect.width, DividerHeight);
        Widgets.DrawBoxSolid(dividerRect, new Color(skin.BorderTintColor.r, skin.BorderTintColor.g, skin.BorderTintColor.b, 0.28f));

        for (int i = 0; i < section.Rows.Count; i++) {
            MeasuredRow row = section.Rows[i];
            DrawRow(row, systemKey, section.Section.Key, skin);
        }

        if (!hasFlash || flash.SectionKey != section.Section.Key) return;

        Rect flashRect = section.Rect;
        if (!flash.SettingKey.NullOrEmpty() && TryGetRow(section, flash.SettingKey!, out MeasuredRow targetedRow)) {
            flashRect = targetedRow.Rect;
        }

        Widgets.DrawBoxSolid(
            flashRect,
            new Color(skin.AccentColor.r, skin.AccentColor.g, skin.AccentColor.b, flashAlpha * 0.14f)
        );
    }

    private void DrawRow(MeasuredRow row, string systemKey, string sectionKey, ISystemSkin skin) {
        try {
            SettingDescriptor descriptor = row.Descriptor;
            if (row.Unavailable) {
                DrawUnavailableRow(row.Rect, skin);
                return;
            }

            bool enabled = descriptor.IsEnabled;
            string? disabledReasonKey = descriptor.DisabledReasonKey;
            float labelWidth = GetLabelWidth(row.Rect.width);
            Rect labelRect = new Rect(row.Rect.x, row.Rect.y + RowVerticalPadding, labelWidth, row.Rect.height - RowVerticalPadding * 2f);
            Rect controlRect = new Rect(labelRect.xMax + 12f, row.Rect.y + RowVerticalPadding, row.Rect.xMax - labelRect.xMax - 12f, row.Rect.height - RowVerticalPadding * 2f);
            Color textColor = enabled ? skin.HeaderTextColor : DisabledTextColor(skin);

            DrawLabel(labelRect, descriptor, textColor);
            controlRenderer.Draw(controlRect, descriptor.Control, systemKey + "/" + sectionKey + "/" + descriptor.Key, skin, enabled, disabledReasonKey);

            if (!enabled) {
                string reason = DisabledReason(disabledReasonKey);
                TooltipHandler.TipRegion(row.Rect, reason);
            } else if (!descriptor.DescriptionKey.NullOrEmpty()) {
                TooltipHandler.TipRegion(row.Rect, descriptor.DescriptionKey.Translate());
            }
        } catch (Exception exception) {
            Logger.Error($"Settings descriptor {systemKey}/{sectionKey}/{row.Descriptor.Key} failed: {exception}");
            DrawUnavailableRow(row.Rect, skin);
        }
    }

    private static void DrawLabel(Rect rect, SettingDescriptor descriptor, Color textColor) {
        string label = descriptor.LabelKey.Translate();
        string? description = descriptor.DescriptionKey.NullOrEmpty() ? null : descriptor.DescriptionKey.Translate();
        float labelHeight;
        using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, textColor)) {
            labelHeight = Text.CalcHeight(label, rect.width);
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, labelHeight), label);
        }

        if (description == null) return;

        Rect descriptionRect = new Rect(rect.x, rect.y + labelHeight, rect.width, rect.height - labelHeight);
        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, new Color(textColor.r, textColor.g, textColor.b, textColor.a * 0.72f))) {
            Widgets.Label(descriptionRect, description);
        }
    }

    private void DrawUnavailableRow(Rect rect, ISystemSkin skin) {
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, DisabledTextColor(skin))) {
            Widgets.Label(rect.LeftPart(0.48f), "CC_Settings_Unavailable".Translate());
        }

        controlRenderer.DrawUnavailable(rect.RightPart(0.48f), skin);
        TooltipHandler.TipRegion(rect, "CC_Settings_Unavailable".Translate());
    }

    private static bool TryGetRow(MeasuredSection section, string settingKey, out MeasuredRow row) {
        for (int i = 0; i < section.Rows.Count; i++) {
            if (section.Rows[i].Descriptor.Key != settingKey) continue;

            row = section.Rows[i];
            return true;
        }

        row = null!;
        return false;
    }

    private MeasuredSection? FindSection(string key) {
        for (int i = 0; i < measuredSections.Count; i++) {
            if (measuredSections[i].Section.Key == key) return measuredSections[i];
        }

        return null;
    }

    private static float GetColumnWidth(float contentWidth) {
        return Mathf.Max(0f, (contentWidth - SettingsWindowLayout.ContentPadding * 2f - ColumnGap) / 2f);
    }

    private static float GetLabelWidth(float rowWidth) {
        return rowWidth * LabelWidthRatio;
    }

    private static string DisabledReason(string? disabledReasonKey) {
        return disabledReasonKey.NullOrEmpty()
            ? "CC_Settings_Unavailable".Translate()
            : disabledReasonKey.Translate();
    }

    private static Color DisabledTextColor(ISystemSkin skin) {
        return new Color(skin.HeaderTextColor.r, skin.HeaderTextColor.g, skin.HeaderTextColor.b, 0.42f);
    }

    private sealed class MeasuredSection {
        public MeasuredSection(SettingSection section, List<MeasuredRow> rows, float height) {
            Section = section;
            Rows = rows;
            Height = height;
        }

        public SettingSection Section { get; }

        public List<MeasuredRow> Rows { get; }

        public float Height { get; }

        public Rect Rect { get; set; }
    }

    private sealed class MeasuredRow {
        public MeasuredRow(SettingDescriptor descriptor, float height, bool unavailable = false) {
            Descriptor = descriptor;
            Height = height;
            Unavailable = unavailable;
        }

        public SettingDescriptor Descriptor { get; }

        public float Height { get; }

        public bool Unavailable { get; }

        public Rect Rect { get; set; }
    }
}
