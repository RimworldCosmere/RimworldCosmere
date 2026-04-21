using Cosmere.Core.UI.Dock;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Skin;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public sealed class FeruchemyDockSection : IDockSection {
    public string SystemId => "Feruchemy";
    public ISystemSkin Skin => SystemSkinRegistry.For(SystemId);

    private const float CellHeight = 36f;
    private const float CompactCellHeight = 22f;
    private const float CellSpacing = 4f;
    private const float HeaderHeight = 28f;
    private const float ButtonWidth = 66f;

    public float GetHeaderHeight() => HeaderHeight;

    public float GetExpandedBodyHeight(Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        float h = ctx.Density == DockDensityMode.Compact ? CompactCellHeight : CellHeight;
        int visible = 0;
        for (int i = 0; i < snapshot.Cells.Count; i++) {
            if (!ctx.TwinbornPairs.ContainsKey(snapshot.Cells[i].SubsystemId)) visible++;
        }
        return visible * (h + CellSpacing);
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
    }

    private void DrawCell(Rect cellRect, Pawn pawn, InvestitureCell cell) {
        bool compact = cellRect.height <= CompactCellHeight + 0.5f;

        Widgets.DrawBoxSolid(cellRect, new Color(0.02f, 0.02f, 0.02f, 0.6f));
        Widgets.DrawBox(cellRect);

        float iconSize = cellRect.height - 8f;
        Rect iconRect = new Rect(cellRect.x + 4f, cellRect.y + 4f, iconSize, iconSize);
        if (cell.Icon != null) GUI.DrawTexture(iconRect, cell.Icon);

        float barHeight = compact ? 10f : 14f;
        Rect barRect = new Rect(
            iconRect.xMax + 6f,
            cellRect.y + cellRect.height / 2f - barHeight / 2f,
            cellRect.width - iconSize - ButtonWidth - 20f,
            barHeight
        );
        HorizontalBar.Draw(
            barRect,
            cell.Bar.Fraction,
            cell.Bar.TargetValue.HasValue && cell.Bar.Max > 0f
                ? cell.Bar.TargetValue.Value / cell.Bar.Max
                : null,
            Skin.BarBackgroundColor,
            Skin.BarFillColor,
            new Color(1f, 1f, 1f, 0.8f)
        );

        Rect btnRect = new Rect(
            cellRect.xMax - ButtonWidth - 4f,
            cellRect.y + 2f,
            ButtonWidth,
            cellRect.height - 4f
        );

        Feruchemist? gene = FindGene(pawn, cell.SubsystemId);
        string label = gene == null
            ? "-"
            : (gene.isTapping
                ? "CC_Dock_Twinborn_Tap"
                : gene.isStoring
                    ? "CC_Dock_Twinborn_Store"
                    : "CC_Dock_Twinborn_TapOrStore").Translate();
        if (Widgets.ButtonText(btnRect, label) && gene != null) {
            ToggleDirection(gene);
            Event.current?.Use();
        }

        if (gene != null) {
            string arrow = gene.isTapping ? "^" : gene.isStoring ? "v" : "-";
            Rect arrowRect = new Rect(cellRect.xMax - 14f, cellRect.y + 2f, 10f, 10f);
            using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, new Color(1f, 0.7f, 0.2f)))
                Widgets.Label(arrowRect, arrow);
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
