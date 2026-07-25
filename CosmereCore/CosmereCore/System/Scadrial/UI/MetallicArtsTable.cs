using System;
using Cosmere.Core.UI;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

/// Lays the metals out as the Metallic Arts table: one block per quadrant, two
/// tiles across, so position carries the same meaning the book's table does.
/// Both arts follow this table, so Allomancy and Feruchemy share the layout and
/// supply their own tile drawing and interaction.
public static class MetallicArtsTable {
    private static float QuadHeaderHeight => Text.LineHeightOf(GameFont.Tiny) + 3f;
    private const float QuadGap = 6f;
    private const float TileGap = 3f;
    private const int Columns = 2;

    public static float HeightFor(
        IReadOnlyList<MetalGroup> groups,
        string? expandedSubsystemId,
        float expandedStripHeight
    ) {
        float height = 0f;
        for (int g = 0; g < groups.Count; g++) {
            MetalGroup group = groups[g];
            if (group.Rows.Count == 0) continue;

            height += QuadHeaderHeight;
            height += RowCount(group.Rows.Count) * (MetalTile.Height + TileGap);
            if (expandedSubsystemId != null && ContainsMetal(group, expandedSubsystemId)) {
                height += expandedStripHeight + TileGap;
            }

            height += QuadGap;
        }

        return height;
    }

    public static void Draw(
        Rect rect,
        IReadOnlyList<MetalGroup> groups,
        Color headerColor,
        string? expandedSubsystemId,
        float expandedStripHeight,
        Action<Rect, MetalRow> drawTile,
        Action<Rect, MetalRow>? drawStrip
    ) {
        float y = rect.y;
        float tileWidth = (rect.width - TileGap * (Columns - 1)) / Columns;

        for (int g = 0; g < groups.Count; g++) {
            MetalGroup group = groups[g];
            if (group.Rows.Count == 0) continue;

            UIText.EllipsisLabel(
                new Rect(rect.x, y, rect.width, QuadHeaderHeight),
                group.LabelKey.Translate(),
                GameFont.Tiny,
                TextAnchor.MiddleLeft,
                headerColor
            );
            y += QuadHeaderHeight;

            for (int r = 0; r < group.Rows.Count; r++) {
                int column = r % Columns;
                bool aloneInRow = column == 0 && r == group.Rows.Count - 1;
                Rect tileRect = new Rect(
                    rect.x + column * (tileWidth + TileGap),
                    y,
                    aloneInRow ? rect.width : tileWidth,
                    MetalTile.Height
                );
                drawTile(tileRect, group.Rows[r]);

                bool endOfRow = column == Columns - 1 || r == group.Rows.Count - 1;
                if (!endOfRow) continue;

                y += MetalTile.Height + TileGap;

                // The strip drops in beneath whichever row holds the open metal,
                // so it never separates a tile from its neighbour.
                if (expandedSubsystemId == null || drawStrip == null) continue;
                int rowStart = r - column;
                for (int i = rowStart; i <= r; i++) {
                    if (group.Rows[i].Cell.SubsystemId != expandedSubsystemId) continue;
                    Rect stripRect = new Rect(rect.x, y, rect.width, expandedStripHeight);
                    drawStrip(stripRect, group.Rows[i]);
                    y += expandedStripHeight + TileGap;
                    break;
                }
            }

            y += QuadGap;
        }
    }

    private static int RowCount(int tiles) {
        return (tiles + Columns - 1) / Columns;
    }

    private static bool ContainsMetal(MetalGroup group, string subsystemId) {
        for (int i = 0; i < group.Rows.Count; i++) {
            if (group.Rows[i].Cell.SubsystemId == subsystemId) return true;
        }

        return false;
    }
}
