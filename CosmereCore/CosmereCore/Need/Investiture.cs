using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereCore.Need;

public class Investiture : RimWorld.Need {
    private const int MaxInvestiture = 1000000;

    // These thresholds match the canon Heightenings from Warbreaker
    public static readonly int[] BreathEquivalentUnitThresholds = {
        1, // Degree 0: Invested
        100, // 1st Heightening
        200, // 2nd
        600, // 3rd
        1000, // 4th
        2000, // 5th
        3500, // 6th
        5000, // 7th
        6000, // 8th
        10000, // 9th
        50000, // 10th
    };

    public static readonly string[] HeighteningLabels = {
        "Unheightened",
        "1st Heightening",
        "2nd Heightening",
        "3rd Heightening",
        "4th Heightening",
        "5th Heightening",
        "6th Heightening",
        "7th Heightening",
        "8th Heightening",
        "9th Heightening",
        "10th Heightening",
    };


    public Investiture(Pawn pawn) : base(pawn) {
        threshPercents = new List<float> { 0.1f, 0.25f, 0.5f, 0.75f };
    }

    public override float MaxLevel {
        get {
            float cur = CurLevel;
            foreach (int threshold in BreathEquivalentUnitThresholds) {
                if (cur < threshold)
                    return threshold;
            }

            // If above all thresholds, use the last one as the cap
            return MaxInvestiture;
        }
    }

    public override bool ShowOnNeedList =>
        pawn != null && pawn.story.traits.HasTrait(TraitDef.Named("Cosmere_Invested"));

    // ReSharper disable once InconsistentNaming
    public static int GetBreathEquivalentUnitsFromDegree(int degree) {
        return BreathEquivalentUnitThresholds[degree];
    }


    // ReSharper disable once InconsistentNaming
    public static int GetDegreeFromBreathEquivalentUnits(int beu) {
        for (int i = BreathEquivalentUnitThresholds.Length - 1; i >= 0; i--) {
            if (beu >= BreathEquivalentUnitThresholds[i]) {
                return i;
            }
        }

        return 0;
    }

    public override void SetInitialLevel() {
        CurLevel = 0f; // start uninvested
    }

    public override void NeedInterval() {
        if (IsFrozen) return;

        // It should only fall in specific cases. I'll need to figure this out
        // e.g. 
        //   * Allomancers should slowly lose their investiture over the day
        //   * Feruchemists lose it at all (their abilities make them lose it)
        //   * Radiants should quickly lose their investiture over the day

        /*
         * Pseudo code
                if (HasGene("Allomancer"))
                {
                    CurLevel -= FallPerTick * 150f; // 150 ticks between calls
                    CurLevel = Mathf.Clamp01(CurLevel);
                }

                if (HasGene("Radiant"))
                {
                    CurLevel -= FallPerTick * 40f; // 40 ticks between calls
                    CurLevel = Mathf.Clamp01(CurLevel);
                }
         */
    }

    public override string GetTipString() {
        int beu = Mathf.FloorToInt(CurLevel);
        int degree = GetDegreeFromBreathEquivalentUnits(beu);
        string tier = HeighteningLabels[Mathf.Clamp(degree, 0, HeighteningLabels.Length - 1)];

        Color color = Color.Lerp(Color.gray, new Color(0.4f, 0.9f, 1.0f), degree / 10f);
        string? coloredLevel = $"{CurLevel:F0}".Colorize(color);
        string? coloredHeightening = tier.Colorize(color);

        return "CS_Current_Investiture".Translate(
                coloredLevel.Named("LEVEL"),
                coloredHeightening.Named("HEIGHT")
            )
            .Resolve();
    }

    public override void DrawOnGUI(
        Rect rect,
        int maxThresholdMarkers = int.MaxValue,
        float customMargin = -1f,
        bool drawArrows = true,
        bool doTooltip = true,
        Rect? rectForTooltip = null,
        bool drawLabel = true
    ) {
        if (rect.height > 70.0) {
            float num = (float)((rect.height - 70.0) / 2.0);
            rect.height = 70f;
            rect.y += num;
        }


        Rect tooltipRect = rectForTooltip ?? rect;
        if (Mouse.IsOver(tooltipRect))
            Widgets.DrawHighlight(tooltipRect);

        if (doTooltip && Mouse.IsOver(tooltipRect))
            TooltipHandler.TipRegion(tooltipRect, new TipSignal((Func<string>)GetTipString, tooltipRect.GetHashCode()));

        float labelHeight = 14f;
        float labelMargin = customMargin >= 0.0 ? customMargin : labelHeight + 15f;
        if (rect.height < 50.0)
            labelHeight *= Mathf.InverseLerp(0.0f, 50f, rect.height);

        if (drawLabel) {
            using (new TextBlock(GameFont.Small, TextAnchor.LowerLeft)) {
                Widgets.Label(
                    new Rect(
                        (float)(rect.x + (double)labelMargin + rect.width * 0.10000000149011612),
                        rect.y,
                        (float)(rect.width - (double)labelMargin - rect.width * 0.10000000149011612),
                        rect.height / 2f
                    ),
                    LabelCap
                );
            }
        }

        Rect valueRect = rect;
        if (drawLabel) {
            valueRect.y += rect.height / 2f;
            valueRect.height -= rect.height / 2f;
        }

        valueRect = new Rect(
            valueRect.x + labelMargin,
            valueRect.y,
            valueRect.width - labelMargin * 2f,
            valueRect.height - labelHeight
        );
        if (DebugSettings.ShowDevGizmos) {
            float lineHeight = Text.LineHeight;
            Rect rect2 = new Rect(valueRect.xMax - lineHeight, valueRect.y - lineHeight, lineHeight, lineHeight);
            if (Widgets.ButtonImage(rect2.ContractedBy(4f), TexButton.Plus))
                OffsetDebugPercent(0.1f);
            if (Mouse.IsOver(rect2))
                TooltipHandler.TipRegion(rect2, (TipSignal)"+ 10%");
            Rect rect3 = new Rect(rect2.xMin - lineHeight, valueRect.y - lineHeight, lineHeight, lineHeight);
            if (Widgets.ButtonImage(rect3.ContractedBy(4f), TexButton.Minus))
                OffsetDebugPercent(-0.1f);
            if (Mouse.IsOver(rect3))
                TooltipHandler.TipRegion(rect3, (TipSignal)"- 10%");
        }

        valueRect.width += 20f;

        int beu = Mathf.FloorToInt(CurLevel);
        int degree = GetDegreeFromBreathEquivalentUnits(beu);
        string tier = HeighteningLabels[Mathf.Clamp(degree, 0, HeighteningLabels.Length - 1)];

        string text = $"{tier} ({beu} BEUs)";

        // Optional: assign a color gradient based on Heightening
        Color color = Color.Lerp(Color.gray, new Color(0.4f, 0.9f, 1.0f), degree / 10f);
        string colored = text.Colorize(color);

        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter)) {
            Widgets.Label(valueRect, colored);
        }
    }
}