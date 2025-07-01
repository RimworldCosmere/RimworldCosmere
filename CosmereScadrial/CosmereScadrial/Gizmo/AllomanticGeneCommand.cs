using System.Collections.Generic;
using System.Linq;
using System.Text;
using CosmereScadrial.Allomancy.Ability;
using CosmereScadrial.Allomancy.Comp.Thing;
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

    private new Allomancer gene => (Allomancer)base.gene;
    private MetalBurning burning => pawn.GetComp<MetalBurning>();
    protected override int IncrementDivisor => 5;

    protected override string GetTooltipHeader() {
        float rate = burning.GetTotalBurnRate(metal) * GenTicks.TicksPerRealSecond;
        NamedArgument coloredCount =
            gene.RequestedVialStock.ToString().Colorize(ColoredText.FactionColor_Ally).Named("COUNT");
        StringBuilder tooltip = new StringBuilder(base.GetTooltipHeader());

        if (burning.IsBurning(metal)) {
            tooltip.AppendLine("CS_BurnRate".Translate($"{rate:0.000}".Named("RATE"))
                .Colorize(ColoredText.FactionColor_Hostile));
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
        NamedArgument coloredPawn = pawn.NameShortColored.Named("PAWN");
        NamedArgument coloredMetal = metal.coloredLabel.Named("METAL");
        StringBuilder tooltip = new StringBuilder(base.GetTooltipFooter());

        if (IsDraggable) {
            tooltip.AppendLine("\n" + "CS_ChangeVialStock".Translate(coloredPawn, coloredMetal, pawn.Named("pawn"))
                .Resolve());
            tooltip.AppendLine("\n" + "CS_SetVialLevelTooltip".Translate(coloredPawn, coloredMetal).Resolve());
        }

        return tooltip.ToString();
    }

    protected override Texture2D GetIcon() {
        return metal.allomancy!.invertedIcon;
    }

    protected override Rect DrawIconBox(Rect iconBoxRect, ref bool mouseOverElement) {
        const float configureButtonSize = 20f;
        NamedArgument coloredPawn = pawn.NameShortColored.Named("PAWN");
        NamedArgument coloredMetal = metal.coloredLabel.Named("METAL");

        Rect rect = base.DrawIconBox(iconBoxRect, ref mouseOverElement);

        Rect configureRect = new Rect(rect.xMax - configureButtonSize, rect.yMin + 2,
            configureButtonSize, configureButtonSize);
        TooltipHandler.TipRegion(configureRect,
            (TaggedString)"CS_SetVialCountTooltip".Translate(coloredPawn, coloredMetal, pawn.Named("pawn")).Resolve());
        if (Mouse.IsOver(configureRect)) {
            mouseOverElement = true;
        }

        if (Widgets.ButtonImageFitted(configureRect, VialIcon)) {
            const int min = 0;
            const int max = 20;
            Dialog_Slider slider = new Dialog_Slider(
                amount => "CS_SetVialCountSlider"
                    .Translate(amount.Named("COUNT"), coloredPawn, coloredMetal, pawn.Named("pawn")).Resolve(),
                min,
                max,
                value => gene.RequestedVialStock = value,
                gene.RequestedVialStock
            );
            Find.WindowStack.Add(slider);
        }

        return rect;
    }

    protected override void Initialize() {
        if (initialized) {
            return;
        }

        base.Initialize();

        subgizmos = gene.def.abilities
            .OrderBy(x => x.uiOrder)
            .Select(x => pawn.abilities.GetAbility(x))
            .Cast<AbstractAbility>()
            .Select(x => new AbilitySubGizmo(this, gene, x))
            .ToList<SubGizmo>();
    }

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms) {
        GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
        bool mouseOver = result.State.Equals(GizmoState.Mouseover);

        /*if (!Title.NullOrEmpty()) {
            using (new TextBlock(GameFont.Tiny)) {
                float textWidth = iconRect.width;
                float textHeight = Text.CalcHeight(Title, textWidth);
                Rect labelRect = new Rect(iconRect.x, mainRect.yMax - textHeight, textWidth, textHeight);
                GUI.DrawTexture(labelRect, TexUI.GrayTextBG);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(labelRect, Title);
            }
        }

        // @todo Maybe implement float menu stuff?

        if (Mouse.IsOver(mainRect) && !mouseOver) {
            Widgets.DrawHighlight(mainRect);
            TooltipHandler.TipRegion(mainRect, GetTooltip, Gen.HashCombineInt(GetHashCode(), 8491284));
        }*/

        return new GizmoResult(mouseOver ? GizmoState.Mouseover : GizmoState.Clear, result.InteractEvent);
    }
}