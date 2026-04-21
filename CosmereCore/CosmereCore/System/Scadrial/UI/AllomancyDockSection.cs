using Cosmere.Core.Ability;
using Cosmere.Core.UI.Dock;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Skin;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public sealed class AllomancyDockSection : IDockSection {
    public string SystemId => "Allomancy";
    public ISystemSkin Skin => SystemSkinRegistry.For(SystemId);

    private const float CellHeight = 36f;
    private const float CompactCellHeight = 22f;
    private const float CellSpacing = 4f;
    private const float HeaderHeight = 28f;
    private const float BurnButtonWidth = 52f;

    public float GetHeaderHeight() => HeaderHeight;

    public float GetExpandedBodyHeight(Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        float h = ctx.Density == DockDensityMode.Compact ? CompactCellHeight : CellHeight;
        int soloCells = 0;
        for (int i = 0; i < snapshot.Cells.Count; i++) {
            if (!ctx.TwinbornPairs.ContainsKey(snapshot.Cells[i].SubsystemId)) soloCells++;
        }
        int pairCells = ctx.TwinbornPairs.Count;
        return soloCells * (h + CellSpacing)
               + pairCells * (TwinbornCell.Height + TwinbornCell.Spacing);
    }

    public void DrawHeader(Rect rect, bool expanded) {
        Widgets.DrawBoxSolid(rect, new Color(Skin.AccentColor.r, Skin.AccentColor.g, Skin.AccentColor.b, 0.25f));
        using (new TextBlock(Skin.HeaderFont, TextAnchor.MiddleLeft, Skin.HeaderTextColor))
            Widgets.Label(rect.ContractedBy(6f, 0f), Skin.HeaderLabel + (expanded ? " -" : " +"));
    }

    public void DrawBody(Rect rect, Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        float h = ctx.Density == DockDensityMode.Compact ? CompactCellHeight : CellHeight;
        float y = rect.y;

        for (int i = 0; i < snapshot.Cells.Count; i++) {
            InvestitureCell cell = snapshot.Cells[i];
            if (ctx.TwinbornPairs.ContainsKey(cell.SubsystemId)) continue;

            Rect cellRect = new Rect(rect.x + 4f, y, rect.width - 8f, h);
            DrawCell(cellRect, pawn, cell);
            y += h + CellSpacing;
        }

        ISystemSkin feruchemySkin = SystemSkinRegistry.For("Feruchemy");
        foreach (KeyValuePair<string, TwinbornPair> kv in ctx.TwinbornPairs) {
            Rect pairRect = new Rect(rect.x + 4f, y, rect.width - 8f, TwinbornCell.Height);
            TwinbornCell.Draw(pairRect, pawn, kv.Value, Skin, feruchemySkin);
            y += TwinbornCell.Height + TwinbornCell.Spacing;
        }
    }

    private void DrawCell(Rect cellRect, Pawn pawn, InvestitureCell cell) {
        bool compact = cellRect.height <= CompactCellHeight + 0.5f;

        Widgets.DrawBoxSolid(cellRect, new Color(0.02f, 0.02f, 0.02f, 0.6f));
        Widgets.DrawBox(cellRect);

        float iconSize = cellRect.height - 8f;
        Rect iconRect = new Rect(cellRect.x + 4f, cellRect.y + 4f, iconSize, iconSize);
        if (cell.Icon != null) GUI.DrawTexture(iconRect, cell.Icon);

        Rect barRect = new Rect(
            iconRect.xMax + 6f,
            cellRect.y + cellRect.height / 2f - 5f,
            cellRect.width - iconSize - BurnButtonWidth - 20f,
            10f
        );
        float? targetFraction = (!compact || Mouse.IsOver(cellRect))
            ? (cell.Bar.TargetValue.HasValue && cell.Bar.Max > 0f
                ? cell.Bar.TargetValue.Value / cell.Bar.Max
                : (float?)null)
            : null;
        HorizontalBar.Draw(
            barRect,
            cell.Bar.Fraction,
            targetFraction,
            Skin.BarBackgroundColor,
            Skin.BarFillColor,
            new Color(1f, 1f, 1f, 0.8f)
        );

        Rect burnRect = new Rect(
            cellRect.xMax - BurnButtonWidth - 4f,
            cellRect.y + 2f,
            BurnButtonWidth,
            cellRect.height - 4f
        );
        string label = (cell.IsActive ? "CC_Dock_Twinborn_Stop" : "CC_Dock_Twinborn_Burn").Translate();
        if (Widgets.ButtonText(burnRect, label)) {
            ToggleBurn(pawn, cell.SubsystemId, Event.current != null && Event.current.shift);
            Event.current?.Use();
        }

        if (cell.IsActive) {
            Rect dot = new Rect(cellRect.xMax - 10f, cellRect.y + 2f, 6f, 6f);
            Widgets.DrawBoxSolid(dot, new Color(1f, 0.7f, 0.2f));
        }
        if (cell.IsFlaring) {
            Widgets.DrawBox(cellRect, 2);
        }

        if (Widgets.ButtonInvisible(cellRect, doMouseoverSound: false) && Event.current != null && Event.current.button == 1) {
            OpenContextMenu(pawn, cell);
            Event.current.Use();
        }
    }

    private static void ToggleBurn(Pawn pawn, string metalDefName, bool flare) {
        if (pawn.abilities == null) return;
        List<Ability> all = pawn.abilities.AllAbilitiesForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is AllomancyAbility a && a.metal.defName == metalDefName) {
                Status next = a.atLeastBurning
                    ? BurningStatus.Off
                    : flare ? BurningStatus.Flaring : BurningStatus.Burning;
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
