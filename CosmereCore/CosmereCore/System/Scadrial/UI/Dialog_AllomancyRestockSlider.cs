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

    public override Vector2 InitialSize => new Vector2(340f, 110f);

    public override void DoWindowContents(Rect inRect) {
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, Color.white))
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 24f),
                $"Restock threshold - {gene.metal.LabelCap}");

        Rect sliderRect = new Rect(inRect.x, inRect.y + 32f, inRect.width, 24f);
        float newValue = Widgets.HorizontalSlider(sliderRect, gene.targetValue, 0f, gene.Max, middleAlignment: true);
        if (!Mathf.Approximately(newValue, gene.targetValue)) {
            gene.targetValue = newValue;
        }

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperRight, new Color(0.8f, 0.8f, 0.8f)))
            Widgets.Label(new Rect(inRect.x, inRect.y + 60f, inRect.width, 20f),
                $"{gene.targetValue:F0} / {gene.Max:F0}");
    }
}
