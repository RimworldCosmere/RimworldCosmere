using Cosmere.Core.UI.Dock;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Skin;
using Cosmere.System.Roshar.Dialog;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.UI;

public sealed class SurgebindingDockSection : IDockSection {
    public string SystemId => "Surgebinding";
    public ISystemSkin Skin => SystemSkinRegistry.For(SystemId);

    private const float HeaderHeight = 28f;
    private const float BodyHeight = 112f;
    private const float PipSize = 10f;
    private const float InfoButtonSize = 20f;

    public float GetHeaderHeight() => HeaderHeight;

    public float GetExpandedBodyHeight(Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) => BodyHeight;

    public void DrawHeader(Rect rect, bool expanded) {
        Widgets.DrawBoxSolid(rect, new Color(Skin.AccentColor.r, Skin.AccentColor.g, Skin.AccentColor.b, 0.25f));
        using (new TextBlock(Skin.HeaderFont, TextAnchor.MiddleLeft, Skin.HeaderTextColor))
            Widgets.Label(rect.ContractedBy(6f, 0f), Skin.HeaderLabel + (expanded ? " -" : " +"));
    }

    public void DrawBody(Rect rect, Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        Surgebinder? gene = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (gene == null || snapshot.PrimaryBar == null) return;

        Rect infoRect = new Rect(rect.xMax - InfoButtonSize - 4f, rect.y + 4f, InfoButtonSize, InfoButtonSize);
        if (Widgets.ButtonText(infoRect, "i")) {
            Find.WindowStack.Add(new RadiantOrderInfoDialog(pawn, gene, RadiantOrderInfoMode.View));
        }

        Rect barRect = new Rect(rect.x + 6f, rect.y + 8f, rect.width - InfoButtonSize - 20f, 14f);
        HorizontalBar.Draw(
            barRect,
            snapshot.PrimaryBar.Fraction,
            snapshot.PrimaryBar.TargetValue.HasValue && snapshot.PrimaryBar.Max > 0f
                ? snapshot.PrimaryBar.TargetValue.Value / snapshot.PrimaryBar.Max
                : null,
            Skin.BarBackgroundColor,
            Skin.BarFillColor,
            new Color(1f, 1f, 1f, 0.8f)
        );

        Rect pipRow = new Rect(rect.x + 6f, barRect.yMax + 6f, rect.width - 12f, PipSize + 2f);
        DrawIdealPips(pipRow, gene);

        Rect orderRow = new Rect(rect.x + 6f, pipRow.yMax + 6f, rect.width - 12f, 18f);
        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleLeft, Skin.HeaderTextColor))
            Widgets.Label(orderRow, gene.radiantOrderDef.LabelCap);

        Rect sliderRow = new Rect(rect.x + 6f, orderRow.yMax + 4f, rect.width - 12f, 20f);
        float newTarget = Widgets.HorizontalSlider(sliderRow, gene.targetValue, 0f, gene.Max, middleAlignment: true);
        if (!Mathf.Approximately(newTarget, gene.targetValue)) {
            gene.targetValue = newTarget;
        }

        Rect sliderLabel = new Rect(rect.x + 6f, sliderRow.yMax, rect.width - 12f, 14f);
        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleRight, new Color(0.75f, 0.85f, 1f)))
            Widgets.Label(sliderLabel, $"refill below {gene.targetValue:F0}");
    }

    private void DrawIdealPips(Rect row, Surgebinder gene) {
        int total = gene.radiantOrderDef.ideals.Count;
        int current = Mathf.Clamp(gene.currentIdealDisplay, 0, total);
        float x = row.x;
        for (int i = 0; i < total; i++) {
            Rect pip = new Rect(x, row.y + (row.height - PipSize) / 2f, PipSize, PipSize);
            Color fill = i < current ? Skin.AccentColor : new Color(0.2f, 0.25f, 0.3f);
            Widgets.DrawBoxSolid(pip, fill);
            Widgets.DrawBox(pip);
            x += PipSize + 4f;
        }
    }
}
