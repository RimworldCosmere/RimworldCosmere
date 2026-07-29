using System;
using Cosmere.Core.UI;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

// Lays the metals out as the Metallic Arts table: one block per quadrant, two
// tiles across, so position carries the same meaning the book's table does.
// Both arts follow this table, so Allomancy and Feruchemy share the layout and
// supply their own tile drawing and interaction.
public static class MetallicArtsTable {
    private static float QuadHeaderHeight => Text.LineHeightOf(GameFont.Tiny) + 3f;

    private const float QuadGap = 6f;
    private const float TileGap = 3f;
    private const int Columns = 2;

    // The open tile runs down through this gap to reach its detail panel, so the
    // panel clears the closed tile beside it rather than butting against the whole
    // row. Only the open tile crosses it, which is what makes the pair read as one
    // shape and its neighbour as a separate one.
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

                // The strip drops in beneath whichever row holds the open metal, so it
                // never separates a tile from its neighbour - and it sits a join gap
                // below that row, which the open tile alone reaches across. The pair
                // reads as one surface; the closed tile beside it keeps its clearance.
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

                // The strip is handed the open tile's own rect, because that span is
                // where its top edge has to stay open for the two to merge.
                int openColumn = openIndex % Columns;
                Rect openTileRect = new Rect(
                    rect.x + openColumn * (tileWidth + TileGap),
                    rowY,
                    openColumn == 0 && openIndex == group.Rows.Count - 1 ? rect.width : tileWidth,
                    MetalTile.Height
                );

                Rect stripRect = new Rect(rect.x, y + JoinGap, rect.width, expandedStripHeight);
                if (expandedStripHeight < stripFullHeight - 0.5f) {
                    // Mid-reveal. The panel is drawn at its finished size inside a clip
                    // only as tall as it has opened so far, so it slides out from under
                    // the tile rather than squashing its contents into a sliver.
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
