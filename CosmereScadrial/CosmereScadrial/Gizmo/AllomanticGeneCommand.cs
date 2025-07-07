using System.Collections.Generic;
using System.Text;
using CosmereScadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Gizmo;

[StaticConstructorOnStartup]
public class AllomanticGeneCommand(
    Gene_Resource gene,
    List<IGeneResourceDrain> drainGenes,
    Color barColor,
    Color barHighlightColor
) : CosmereGeneCommand(gene, drainGenes, barColor, barHighlightColor) {
    private static readonly Texture2D VialIcon =
        ContentFinder<Texture2D>.Get("Things/Item/AllomanticVial/AllomanticVial_c");

    private string? setVialCountTooltipCache;

    private new Allomancer gene => (Allomancer)base.gene;
    protected override int IncrementDivisor => 5;

    private string setVialCountTooltip => setVialCountTooltipCache ??=
        "CS_SetVialCountTooltip".Translate(coloredPawn, coloredMetal, pawn.Named("pawn")).Resolve();

    protected override string GetTooltipHeader() {
        float rate = gene.BurnRate * GenTicks.TicksPerRealSecond;
        NamedArgument coloredCount =
            gene.requestedVialStock.ToString().Colorize(ColoredText.FactionColor_Ally).Named("COUNT");
        StringBuilder tooltip = new StringBuilder(base.GetTooltipHeader() + "\n");

        if (gene.Burning) {
            tooltip.AppendLine(
                "CS_BurnRate".Translate($"{rate:0.000}".Named("RATE"))
                    .Colorize(ColoredText.FactionColor_Hostile)
            );
        }

        if (IsDraggable) {
            tooltip.AppendLine("CS_CurrentVialStock".Translate(coloredCount));
            if (gene.targetValue <= 0.0) {
                tooltip.AppendLine("CS_NeverConsumeVial".Translate());
            } else {
                tooltip.AppendLine("CS_ConsumeVialBelow".Translate() + $": {gene.PostProcessValue(gene.targetValue)}%");
            }
        }

        return tooltip.ToString();
    }

    protected override string GetTooltipFooter() {
        if (cachedTooltipFooter != null) return cachedTooltipFooter;

        StringBuilder tooltip = new StringBuilder(base.GetTooltipFooter() + "\n");

        if (IsDraggable) {
            tooltip.AppendLine(
                "\n" +
                "CS_ChangeVialStock".Translate(coloredPawn, coloredMetal, pawn.Named("pawn")).Resolve()
            );
            tooltip.AppendLine("\n" + "CS_SetVialLevelTooltip".Translate(coloredPawn, coloredMetal).Resolve());
        }

        return cachedTooltipFooter = tooltip.ToString();
    }

    protected override Texture2D GetIcon() {
        return metal.allomancy!.invertedIcon;
    }

    protected override Rect DrawIconBox(ref bool mouseOverElement) {
        const float configureButtonSize = 20f;

        Rect rect = base.DrawIconBox(ref mouseOverElement);

        Rect configureRect = new Rect(
            rect.xMax - configureButtonSize,
            rect.yMin + 2,
            configureButtonSize,
            configureButtonSize
        );
        TooltipHandler.TipRegion(configureRect, () => setVialCountTooltip, Gen.HashCombineInt(GetHashCode(), 57912497));

        if (Widgets.ButtonImageFitted(configureRect, VialIcon)) {
            Dialog_Slider slider = new Dialog_Slider(
                amount => "CS_SetVialCountSlider"
                    .Translate(amount.Named("COUNT"), coloredPawn, coloredMetal, pawn.Named("pawn"))
                    .Resolve(),
                0,
                20,
                value => gene.requestedVialStock = value,
                gene.requestedVialStock
            );
            Find.WindowStack.Add(slider);
        }

        if (Mouse.IsOver(configureRect)) mouseOverElement = true;

        return rect;
    }
}