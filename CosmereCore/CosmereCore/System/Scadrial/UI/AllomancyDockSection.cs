using Cosmere.Core.Ability;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Dock;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Skin;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public sealed class AllomancyDockSection : DockSectionBase {
    private const float CellGap = 4f;
    private const float ButtonRowHeight = 20f;
    private const float ButtonGap = 4f;
    private readonly HashSet<string> expandedRows = [];
    private readonly Dictionary<string, string> labelCache = new();
    private IReadOnlyList<MetalGroup>? cachedGroups;
    private int cachedPawnId = -1;
    private int cachedCellCount = -1;

    public override string SystemId => "Allomancy";

    public override float GetHeaderHeight() {
        return 28f;
    }

    public override float GetExpandedBodyHeight(Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        IReadOnlyList<MetalGroup> groups = GroupsFor(pawn, snapshot);
        float height = 0f;

        int active = CountActive(snapshot, ctx);
        if (active > 0) {
            height += DockRows.GroupLabelHeight + active * (DockRows.RichCellHeight + CellGap);
        }

        for (int g = 0; g < groups.Count; g++) {
            MetalGroup group = groups[g];
            height += DockRows.GroupLabelHeight;
            for (int r = 0; r < group.Rows.Count; r++) {
                MetalRow row = group.Rows[r];
                if (ctx.DualInvestiturePairs.ContainsKey(row.Cell.SubsystemId)) continue;

                if (row.Cell.IsActive) {
                    height += DockRows.SlimRowHeight;
                    continue;
                }

                height += expandedRows.Contains(row.Cell.SubsystemId)
                    ? DockRows.RichCellHeight + CellGap
                    : DockRows.SlimRowHeight;
            }
        }

        height += ctx.DualInvestiturePairs.Count * (DualInvestitureCell.Height + DualInvestitureCell.Spacing);

        return height;
    }

    public override void DrawBody(Rect rect, Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        IReadOnlyList<MetalGroup> groups = GroupsFor(pawn, snapshot);
        float y = rect.y;

        int active = CountActive(snapshot, ctx);
        if (active > 0) {
            DockRows.DrawGroupLabel(new Rect(rect.x, y, rect.width, DockRows.GroupLabelHeight), "CC_Dock_Group_Burning".Translate(), true);
            y += DockRows.GroupLabelHeight;
            for (int i = 0; i < snapshot.Cells.Count; i++) {
                InvestitureCell cell = snapshot.Cells[i];
                if (!cell.IsActive || ctx.DualInvestiturePairs.ContainsKey(cell.SubsystemId)) continue;
                Rect cellRect = new Rect(rect.x + 6f, y, rect.width - 12f, DockRows.RichCellHeight);
                DrawRichCell(cellRect, pawn, cell, true);
                y += DockRows.RichCellHeight + CellGap;
            }
        }

        for (int g = 0; g < groups.Count; g++) {
            MetalGroup group = groups[g];
            DockRows.DrawGroupLabel(new Rect(rect.x, y, rect.width, DockRows.GroupLabelHeight), group.LabelKey.Translate(), false);
            y += DockRows.GroupLabelHeight;
            for (int r = 0; r < group.Rows.Count; r++) {
                MetalRow row = group.Rows[r];
                if (ctx.DualInvestiturePairs.ContainsKey(row.Cell.SubsystemId)) continue;

                if (row.Cell.IsActive) {
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

        ISystemSkin feruchemySkin = SystemSkinRegistry.ForOrFallback("Feruchemy");
        foreach (KeyValuePair<string, IDualInvestiturePair> kv in ctx.DualInvestiturePairs) {
            Rect pairRect = new Rect(rect.x + 4f, y, rect.width - 8f, DualInvestitureCell.Height);
            DualInvestitureCell.Draw(pairRect, pawn, kv.Value, Skin, feruchemySkin);
            y += DualInvestitureCell.Height + DualInvestitureCell.Spacing;
        }
    }

    private IReadOnlyList<MetalGroup> GroupsFor(Pawn pawn, InvestitureSnapshot snapshot) {
        if (cachedGroups == null || cachedPawnId != pawn.thingIDNumber || cachedCellCount != snapshot.Cells.Count) {
            cachedGroups = MetalGroupTable.AllomancyGroups(snapshot.Cells);
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

    private static int CountActive(InvestitureSnapshot snapshot, DockRenderContext ctx) {
        int count = 0;
        for (int i = 0; i < snapshot.Cells.Count; i++) {
            InvestitureCell cell = snapshot.Cells[i];
            if (cell.IsActive && !ctx.DualInvestiturePairs.ContainsKey(cell.SubsystemId)) count++;
        }

        return count;
    }

    private void DrawRichCell(Rect cellRect, Pawn pawn, InvestitureCell cell, bool pinned) {
        string stateLabel = pinned
            ? cell.IsFlaring ? "CC_Dock_State_Flaring".Translate() : "CC_Dock_State_Burning".Translate()
            : "";
        Color stateColor = cell.IsFlaring ? DockPalette.Flare : DockPalette.HotLabel;
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
            cell.IsFlaring
        );

        if (Widgets.ButtonInvisible(cellRect, false) && Event.current != null && Event.current.button == 1) {
            OpenContextMenu(pawn, cell);
            Event.current.Use();
        }

        float buttonWidth = (buttonRow.width - 2f * ButtonGap) / 3f;
        Rect burnRect = new Rect(buttonRow.x, buttonRow.y, buttonWidth, ButtonRowHeight);
        Rect thresholdRect = new Rect(burnRect.xMax + ButtonGap, buttonRow.y, buttonWidth, ButtonRowHeight);
        Rect compoundRect = new Rect(thresholdRect.xMax + ButtonGap, buttonRow.y, buttonWidth, ButtonRowHeight);

        string burnLabel = (cell.IsActive ? "CC_Dock_Twinborn_Stop" : "CC_Dock_Twinborn_Burn").Translate();
        if (Widgets.ButtonText(burnRect, burnLabel)) {
            ToggleBurn(pawn, cell.SubsystemId, Event.current != null && Event.current.shift);
            Event.current?.Use();
        }

        if (Widgets.ButtonText(thresholdRect, "CC_Dock_Btn_Threshold".Translate())) {
            OpenContextMenu(pawn, cell);
            Event.current?.Use();
        }

        MetallicArtsMetalDef? metalDef = DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(cell.SubsystemId);
        AllomanticAbilityDef? cpdDef = metalDef?.GetCompoundAbility();
        RimWorld.Ability? cpdAbility = cpdDef != null ? pawn.abilities?.GetAbility(cpdDef) : null;
        bool cpdEnabled = cpdAbility != null && cpdAbility.CanCast;

        Color orig = GUI.color;
        if (!cpdEnabled) GUI.color = new Color(1f, 1f, 1f, 0.35f);
        if (Widgets.ButtonText(compoundRect, "CC_Dock_Twinborn_Compound".Translate(), active: cpdEnabled) && cpdEnabled) {
            cpdAbility!.QueueCastingJob(pawn, LocalTargetInfo.Invalid);
        }

        GUI.color = orig;
    }

    private static void ToggleBurn(Pawn pawn, string metalDefName, bool flare) {
        if (pawn.abilities == null) return;
        List<Ability> all = pawn.abilities.AllAbilitiesForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is AllomancyAbility a && a.metal.defName == metalDefName) {
                Status next = a.atLeastBurning
                    ? BurningStatus.Off
                    : flare
                        ? BurningStatus.Flaring
                        : BurningStatus.Burning;
                a.UpdateStatus(next);
                return;
            }
        }
    }

    private static void OpenContextMenu(Pawn pawn, InvestitureCell cell) {
        if (pawn.genes == null) return;
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Allomancer a && a.metal.defName == cell.SubsystemId && !a.Overridden) {
                Find.WindowStack.Add(new Dialog_AllomancyRestockSlider(a));
                return;
            }
        }
    }
}
