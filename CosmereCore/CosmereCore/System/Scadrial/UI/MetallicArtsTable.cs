using System;
using Cosmere.Core.UI;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

/// Lays metals out as the book's Metallic Arts table: one block per quadrant, two tiles across.
/// Allomancy and Feruchemy share this layout and supply their own tile drawing and interaction.
public static class MetallicArtsTable {
    private static float QuadHeaderHeight => Text.LineHeightOf(GameFont.Tiny) + 3f;

    private const float QuadGap = 6f;
    private const float TileGap = 3f;
    private const int Columns = 2;

    /// The open tile runs through this gap into its detail panel, clearing the closed tile beside it.
    /// Only the open tile crosses it - that is what reads the pair as one shape, its neighbour as separate.
    public const float JoinGap = TileGap;

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
                height += JoinGap + expandedStripHeight;
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
        float stripFullHeight,
        Action<Rect, MetalRow> drawTile,
        Action<Rect, MetalRow, Rect>? drawStrip
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

                // strip sits under the open metal's row, a join gap below it - only that tile reaches across to merge.
                int openIndex = -1;
                if (expandedSubsystemId != null && drawStrip != null) {
                    for (int i = r - column; i <= r; i++) {
                        if (group.Rows[i].Cell.SubsystemId == expandedSubsystemId) openIndex = i;
                    }
                }

                float rowY = y;
                y += MetalTile.Height;
                if (openIndex < 0) {
                    y += TileGap;
                    continue;
                }

                // strip gets the open tile's own rect - its top edge must stay open for the two to merge.
                int openColumn = openIndex % Columns;
                Rect openTileRect = new Rect(
                    rect.x + openColumn * (tileWidth + TileGap),
                    rowY,
                    openColumn == 0 && openIndex == group.Rows.Count - 1 ? rect.width : tileWidth,
                    MetalTile.Height
                );

                Rect stripRect = new Rect(rect.x, y + JoinGap, rect.width, expandedStripHeight);
                if (expandedStripHeight < stripFullHeight - 0.5f) {
                    // mid-reveal: panel draws full size in a clip only as tall as opened - slides out, not squashes.
                    Widgets.BeginGroup(stripRect);
                    drawStrip!(
                        new Rect(0f, 0f, rect.width, stripFullHeight),
                        group.Rows[openIndex],
                        new Rect(openTileRect.x - stripRect.x, 0f, openTileRect.width, openTileRect.height)
                    );
                    Widgets.EndGroup();
                } else {
                    drawStrip!(stripRect, group.Rows[openIndex], openTileRect);
                }

                y += JoinGap + expandedStripHeight + TileGap;
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
