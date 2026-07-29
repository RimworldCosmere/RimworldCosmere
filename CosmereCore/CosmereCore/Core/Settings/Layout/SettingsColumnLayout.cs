using global::System.Collections.Generic;

namespace Cosmere.Core.Settings.Layout;

public static class SettingsColumnLayout {
    public static IReadOnlyList<SettingSectionPlacement> Place(
        IReadOnlyList<SettingSectionMeasurement> sections,
        float columnWidth,
        float gap
    ) {
        List<SettingSectionPlacement> placements = new List<SettingSectionPlacement>(sections.Count);
        float leftHeight = 0f;
        float rightHeight = 0f;

        foreach (SettingSectionMeasurement section in sections) {
            int column = leftHeight <= rightHeight ? 0 : 1;
            float y = column == 0 ? leftHeight : rightHeight;
            float x = column == 0 ? 0f : columnWidth + gap;

            placements.Add(new SettingSectionPlacement(section.Key, column, x, y, columnWidth, section.Height));

            if (column == 0) leftHeight += section.Height + gap;
            else rightHeight += section.Height + gap;
        }

        return placements;
    }
}
