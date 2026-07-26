using System;
using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Need;

public class Investiture : RimWorld.Need {
    public const float MaxInvestiture = float.PositiveInfinity;

    // These thresholds match the canon Heightenings from Warbreaker
    public static readonly int[] BreathEquivalentUnitThresholds = [
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
    ];

    public static readonly string[] HeighteningLabels = [
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
    ];

    private CompGlower? cachedGlower;

    private int unlockedHeightening;

    public Investiture(Pawn pawn) : base(pawn) {
        threshPercents = [0.1f, 0.25f, 0.5f, 0.75f];
    }

    public string InvestitureLabel {
        get {
            Invested? investedGene = pawn.genes?.GetFirstGeneOfType<Invested>();
            if (investedGene != null && !string.IsNullOrEmpty(investedGene.InvestitureLabel)) {
                return investedGene.InvestitureLabel;
            }

            return LabelCap;
        }
    }

    public override float MaxLevel {
        get {
            Invested? investedGene = pawn.genes?.GetFirstGeneOfType<Invested>();
            if (investedGene != null && investedGene.MaxInvestitureLevel > 0) {
                return investedGene.MaxInvestitureLevel;
            }

            for (int i = unlockedHeightening + 1; i < BreathEquivalentUnitThresholds.Length; i++) {
                if (CurLevel < BreathEquivalentUnitThresholds[i]) {
                    return BreathEquivalentUnitThresholds[i];
                }
            }

            return investitureHolder.maxInvestitureSelf;
        }
    }

    private InvestitureHolder investitureHolder => pawn.GetComp<InvestitureHolder>();

    public override float CurLevel {
        get => investitureHolder.currentInvestiture;
        set {
            pawn.records.AddTo(
                value > base.CurLevel
                    ? RecordDefOf.Cosmere_Core_Record_InvestitureGained
                    : RecordDefOf.Cosmere_Core_Record_InvestitureSpent,
                value
            );
            investitureHolder.currentInvestitureSelf = value;
        }
    }

    public bool IsMaxLevel => Mathf.Approximately(CurLevel, investitureHolder.maxInvestiture);

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

    public override void SetInitialLevel() { }

    public override void NeedInterval() {
        pawn.story?.EnsureTrait(TraitDefOf.Cosmere_Invested, GetDegreeFromBreathEquivalentUnits((int)CurLevel));

        if (cachedGlower == null) {
            if (!pawn.TryGetComp(out cachedGlower)) {
                cachedGlower = new CompGlower();
                cachedGlower.parent = pawn;
                cachedGlower.Initialize(
                    new CompProperties_Glower {
                        glowRadius = Mathf.Clamp(CurLevel, 0f, 3f),
                        overlightRadius = 2f,
                        glowColor = new ColorInt(new Color(.25f, .95f, .95f, .2f)),
                    }
                );
            }
        }

        cachedGlower!.GlowRadius = Mathf.Clamp(CurLevel, 0f, 3f);
    }

    public override string GetTipString() {
        int beu = Mathf.FloorToInt(CurLevel);
        int degree = GetDegreeFromBreathEquivalentUnits(beu);
        string tier = HeighteningLabels[Mathf.Clamp(degree, 0, HeighteningLabels.Length - 1)];

        Color color = Color.Lerp(Color.gray, new Color(0.4f, 0.9f, 1.0f), degree / 10f);
        string? coloredLevel = $"{CurLevel:F0}".Colorize(color);
        string? coloredHeightening = tier.Colorize(color);

        return "CC_Need_CurrentInvestiture".Translate(
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
        if (Mouse.IsOver(tooltipRect)) {
            Widgets.DrawHighlight(tooltipRect);
        }

        if (doTooltip && Mouse.IsOver(tooltipRect)) {
            TooltipHandler.TipRegion(tooltipRect, new TipSignal((Func<string>)GetTipString, tooltipRect.GetHashCode()));
        }

        float labelHeight = 14f;
        float labelMargin = customMargin >= 0.0 ? customMargin : labelHeight + 15f;
        if (rect.height < 50.0) {
            labelHeight *= Mathf.InverseLerp(0.0f, 50f, rect.height);
        }

        if (drawLabel) {
            using (new TextBlock(GameFont.Small, TextAnchor.LowerLeft)) {
                Widgets.Label(
                    new Rect(
                        (float)(rect.x + (double)labelMargin + rect.width * 0.10000000149011612),
                        rect.y,
                        (float)(rect.width - (double)labelMargin - rect.width * 0.10000000149011612),
                        rect.height / 2f
                    ),
                    InvestitureLabel
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
            if (Widgets.ButtonImage(rect2.ContractedBy(4f), TexButton.Plus)) {
                OffsetDebugPercent(0.1f);
            }

            if (Mouse.IsOver(rect2)) {
                TooltipHandler.TipRegion(rect2, (TipSignal)"+ 10%");
            }

            Rect rect3 = new Rect(rect2.xMin - lineHeight, valueRect.y - lineHeight, lineHeight, lineHeight);
            if (Widgets.ButtonImage(rect3.ContractedBy(4f), TexButton.Minus)) {
                OffsetDebugPercent(-0.1f);
            }

            if (Mouse.IsOver(rect3)) {
                TooltipHandler.TipRegion(rect3, (TipSignal)"- 10%");
            }
        }

        valueRect.width += 20f;

        int beu = Mathf.FloorToInt(CurLevel);
        int degree = GetDegreeFromBreathEquivalentUnits(beu);
        string text = beu.ToStringBreathEquivalentUnits();

        Color color = Color.Lerp(Color.gray, new Color(0.4f, 0.9f, 1.0f), degree / 10f);
        string colored = text.Colorize(color);

        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter)) {
            Widgets.Label(valueRect, colored);
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref unlockedHeightening, "unlockedHeightening");
    }
}