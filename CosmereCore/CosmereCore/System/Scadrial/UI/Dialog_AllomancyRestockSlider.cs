using Cosmere.System.Scadrial.Gene;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public sealed class Dialog_AllomancyRestockSlider : Window {
    private readonly Allomancer gene;

    public Dialog_AllomancyRestockSlider(Allomancer gene) {
        this.gene = gene;
        doCloseX = true;
        closeOnClickedOutside = true;
        absorbInputAroundWindow = false;
    }

    public override Vector2 InitialSize => new Vector2(340f, 180f);

    public override void DoWindowContents(Rect inRect) {
        float y = inRect.y;

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, Color.white)) {
            Widgets.Label(
                new Rect(inRect.x, y, inRect.width, 24f),
                "CC_Codex_Allomancy_VialSettings_Title".Translate(gene.metal.LabelCap.Named("METAL"))
            );
        }

        y += 30f;

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, new Color(0.85f, 0.85f, 0.85f))) {
            Widgets.Label(
                new Rect(inRect.x, y, inRect.width, 20f),
                "CC_Codex_Allomancy_VialSettings_ThresholdLabel".Translate()
            );
        }

        y += 22f;

        Rect thresholdSliderRect = new Rect(inRect.x, y, inRect.width, 24f);
        float newThreshold = Widgets.HorizontalSlider(thresholdSliderRect, gene.targetValue, 0f, gene.Max, true);
        if (!Mathf.Approximately(newThreshold, gene.targetValue)) {
            gene.targetValue = newThreshold;
        }

        y += 28f;

        string thresholdLabel = Allomancer.ThresholdDisplayLabel(gene);

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperRight, new Color(0.8f, 0.8f, 0.8f)))
            Widgets.Label(new Rect(inRect.x, y, inRect.width, 18f), thresholdLabel);
        y += 22f;

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, new Color(0.85f, 0.85f, 0.85f))) {
            Widgets.Label(
                new Rect(inRect.x, y, inRect.width, 20f),
                "CC_Codex_Allomancy_VialSettings_StockLabel".Translate()
            );
        }

        y += 22f;

        Rect stockSliderRect = new Rect(inRect.x, y, inRect.width, 24f);
        float newStock = Widgets.HorizontalSlider(
            stockSliderRect,
            gene.requestedVialStock,
            0f,
            Allomancer.MaxRequestedVialStock,
            true
        );
        int newStockInt = Mathf.RoundToInt(newStock);
        if (newStockInt != gene.requestedVialStock) {
            gene.requestedVialStock = newStockInt;
        }

        y += 28f;

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperRight, new Color(0.8f, 0.8f, 0.8f))) {
            Widgets.Label(
                new Rect(inRect.x, y, inRect.width, 18f),
                "CS_CurrentVialStock".Translate(gene.requestedVialStock.Named("COUNT"))
            );
        }
    }
}