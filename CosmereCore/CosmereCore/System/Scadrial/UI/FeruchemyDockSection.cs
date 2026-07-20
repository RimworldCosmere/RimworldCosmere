using Cosmere.Core.UI.Dock;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Skin;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Gene;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public sealed class FeruchemyDockSection : DockSectionBase {
    private const float CellGap = 4f;
    private const float ButtonRowHeight = 20f;
    private readonly HashSet<string> expandedRows = [];
    private readonly Dictionary<string, string> labelCache = new();
    private IReadOnlyList<MetalGroup>? cachedGroups;
    private int cachedPawnId = -1;
    private int cachedCellCount = -1;

    public override string SystemId => "Feruchemy";

    public override float GetHeaderHeight() {
        return 28f;
    }

    public override float GetExpandedBodyHeight(Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        IReadOnlyList<MetalGroup> groups = GroupsFor(pawn, snapshot);
        float height = 0f;

        int tapping = CountByDirection(pawn, snapshot, ctx, true);
        if (tapping > 0) {
            height += DockRows.GroupLabelHeight + tapping * (DockRows.RichCellHeight + CellGap);
        }

        int storing = CountByDirection(pawn, snapshot, ctx, false);
        if (storing > 0) {
            height += DockRows.GroupLabelHeight + storing * (DockRows.RichCellHeight + CellGap);
        }

        for (int g = 0; g < groups.Count; g++) {
            MetalGroup group = groups[g];
            height += DockRows.GroupLabelHeight;
            for (int r = 0; r < group.Rows.Count; r++) {
                MetalRow row = group.Rows[r];
                if (ctx.DualInvestiturePairs.ContainsKey(row.Cell.SubsystemId)) continue;

                Feruchemist? gene = FindGene(pawn, row.Cell.SubsystemId);
                if (gene != null && (gene.isTapping || gene.isStoring)) {
                    height += DockRows.SlimRowHeight;
                    continue;
                }

                height += expandedRows.Contains(row.Cell.SubsystemId)
                    ? DockRows.RichCellHeight + CellGap
                    : DockRows.SlimRowHeight;
            }
        }

        return height;
    }

    public override void DrawBody(Rect rect, Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        IReadOnlyList<MetalGroup> groups = GroupsFor(pawn, snapshot);
        float y = rect.y;

        int tapping = CountByDirection(pawn, snapshot, ctx, true);
        if (tapping > 0) {
            DockRows.DrawGroupLabel(new Rect(rect.x, y, rect.width, DockRows.GroupLabelHeight), "CC_Dock_Group_Tapping".Translate(), true);
            y += DockRows.GroupLabelHeight;
            y = DrawPinnedGroup(rect, pawn, snapshot, ctx, y, true);
        }

        int storing = CountByDirection(pawn, snapshot, ctx, false);
        if (storing > 0) {
            DockRows.DrawGroupLabel(new Rect(rect.x, y, rect.width, DockRows.GroupLabelHeight), "CC_Dock_Group_Storing".Translate(), true);
            y += DockRows.GroupLabelHeight;
            y = DrawPinnedGroup(rect, pawn, snapshot, ctx, y, false);
        }

        for (int g = 0; g < groups.Count; g++) {
            MetalGroup group = groups[g];
            DockRows.DrawGroupLabel(new Rect(rect.x, y, rect.width, DockRows.GroupLabelHeight), group.LabelKey.Translate(), false);
            y += DockRows.GroupLabelHeight;
            for (int r = 0; r < group.Rows.Count; r++) {
                MetalRow row = group.Rows[r];
                if (ctx.DualInvestiturePairs.ContainsKey(row.Cell.SubsystemId)) continue;

                Feruchemist? gene = FindGene(pawn, row.Cell.SubsystemId);
                if (gene != null && (gene.isTapping || gene.isStoring)) {
                    Rect ghostRect = new Rect(rect.x, y, rect.width, DockRows.SlimRowHeight);
                    DockRows.DrawSlimRow(ghostRect, row.Cell.Icon, MetalLabel(row.Cell), row.AxisGlyph, row.Cell.Bar.Fraction, Skin, true);
                    y += DockRows.SlimRowHeight;
                    continue;
                }

                if (expandedRows.Contains(row.Cell.SubsystemId)) {
                    Rect cellRect = new Rect(rect.x + 6f, y, rect.width - 12f, DockRows.RichCellHeight);
                    DrawRichCell(cellRect, pawn, row.Cell, false);
                    y += DockRows.RichCellHeight + CellGap;
                    continue;
                }

                Rect rowRect = new Rect(rect.x, y, rect.width, DockRows.SlimRowHeight);
                if (DockRows.DrawSlimRow(rowRect, row.Cell.Icon, MetalLabel(row.Cell), row.AxisGlyph, row.Cell.Bar.Fraction, Skin, false)) {
                    expandedRows.Add(row.Cell.SubsystemId);
                }

                y += DockRows.SlimRowHeight;
            }
        }
    }

    private float DrawPinnedGroup(Rect rect, Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx, float y, bool tapping) {
        for (int i = 0; i < snapshot.Cells.Count; i++) {
            InvestitureCell cell = snapshot.Cells[i];
            if (ctx.DualInvestiturePairs.ContainsKey(cell.SubsystemId)) continue;

            Feruchemist? gene = FindGene(pawn, cell.SubsystemId);
            if (gene == null) continue;
            if (tapping && !gene.isTapping) continue;
            if (!tapping && !gene.isStoring) continue;

            Rect cellRect = new Rect(rect.x + 6f, y, rect.width - 12f, DockRows.RichCellHeight);
            DrawRichCell(cellRect, pawn, cell, true);
            y += DockRows.RichCellHeight + CellGap;
        }

        return y;
    }

    private IReadOnlyList<MetalGroup> GroupsFor(Pawn pawn, InvestitureSnapshot snapshot) {
        if (cachedGroups == null || cachedPawnId != pawn.thingIDNumber || cachedCellCount != snapshot.Cells.Count) {
            cachedGroups = MetalGroupTable.FeruchemyGroups(snapshot.Cells);
            cachedPawnId = pawn.thingIDNumber;
            cachedCellCount = snapshot.Cells.Count;
        }

        return cachedGroups;
    }

    private string MetalLabel(InvestitureCell cell) {
        if (labelCache.TryGetValue(cell.SubsystemId, out string? cached)) return cached;
        string label = DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(cell.SubsystemId)?.LabelCap ?? cell.SubsystemId;
        labelCache[cell.SubsystemId] = label;
        return label;
    }

    private static int CountByDirection(Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx, bool tapping) {
        int count = 0;
        for (int i = 0; i < snapshot.Cells.Count; i++) {
            InvestitureCell cell = snapshot.Cells[i];
            if (ctx.DualInvestiturePairs.ContainsKey(cell.SubsystemId)) continue;

            Feruchemist? gene = FindGene(pawn, cell.SubsystemId);
            if (gene == null) continue;
            if (tapping ? gene.isTapping : gene.isStoring) count++;
        }

        return count;
    }

    private void DrawRichCell(Rect cellRect, Pawn pawn, InvestitureCell cell, bool pinned) {
        Feruchemist? gene = FindGene(pawn, cell.SubsystemId);
        string stateLabel = pinned && gene != null
            ? (gene.isTapping ? "^ " + "CC_Dock_Twinborn_Tap".Translate() : "v " + "CC_Dock_Twinborn_Store".Translate())
            : "";
        Color stateColor = DockPalette.HotLabel;
        float? targetFraction = cell.Bar.TargetValue.HasValue && cell.Bar.Max > 0f
            ? cell.Bar.TargetValue.Value / cell.Bar.Max
            : null;

        Rect buttonRow = DockRows.BeginRichCell(
            cellRect,
            cell.Icon,
            MetalLabel(cell),
            stateLabel,
            stateColor,
            cell.Bar.Fraction,
            targetFraction,
            Skin,
            false
        );

        if (!pinned) {
            Rect nameStrip = new Rect(cellRect.x, cellRect.y, cellRect.width, ButtonRowHeight);
            if (Widgets.ButtonInvisible(nameStrip) && Event.current != null && Event.current.button == 0) {
                expandedRows.Remove(cell.SubsystemId);
                Event.current.Use();
            }
        }

        float buttonWidth = buttonRow.width / 2f;
        Rect toggleRect = new Rect(buttonRow.x, buttonRow.y, buttonWidth, ButtonRowHeight);

        string label = gene == null
            ? "-"
            : (gene.isTapping
                ? "CC_Dock_Twinborn_Tap"
                : gene.isStoring
                    ? "CC_Dock_Twinborn_Store"
                    : "CC_Dock_Twinborn_TapOrStore").Translate();
        if (Widgets.ButtonText(toggleRect, label) && gene != null) {
            ToggleDirection(gene);
            Event.current?.Use();
        }
    }

    private static Feruchemist? FindGene(Pawn pawn, string metalDefName) {
        if (pawn.genes == null) return null;
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Feruchemist f && f.metal.defName == metalDefName && !f.Overridden) return f;
        }

        return null;
    }

    private static void ToggleDirection(Feruchemist gene) {
        if (gene.isTapping) {
            gene.targetValue = 75f;
            return;
        }

        if (gene.isStoring) {
            gene.Reset();
            return;
        }

        gene.targetValue = 25f;
    }
}
