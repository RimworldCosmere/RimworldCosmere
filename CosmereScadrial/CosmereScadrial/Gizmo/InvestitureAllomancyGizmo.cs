using System;
using System.Collections.Generic;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using CosmereScadrial.Genes;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Gizmo;

[StaticConstructorOnStartup]
public class InvestitureAllomancyGizmo(
    Gene_Resource gene,
    List<IGeneResourceDrain> drainGenes,
    Color barColor,
    Color barHighlightColor) : GeneGizmo_Resource(gene, [], barColor, barHighlightColor) {
    private const float ABILITY_ICON_SIZE = 24;
    private const float ABILITY_ICON_PADDING = 4;

    protected MetallicArtsMetalDef metal => ((Metalborn)gene).metal;
    protected Pawn pawn => gene.pawn;
    protected MetalBurning burning => pawn.GetComp<MetalBurning>();
    protected override bool IsDraggable => gene.pawn.IsColonistPlayerControlled || gene.pawn.IsPrisonerOfColony;
    protected override string BarLabel => $"{ValuePercent:F}%";
    protected override int Increments => gene.MaxForDisplay / 5;
    public override bool Visible => pawn.Faction.IsPlayer;

    protected override string Title => metal.LabelCap;

    protected override string GetTooltip() {
        float rate = burning.GetTotalBurnRate(metal) * GenTicks.TicksPerRealSecond;

        string tooltip =
            $"{gene.ResourceLabel.CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor)}: {gene.ValuePercent:F}%\n";
        if (burning.IsBurning(metal)) {
            tooltip += "BurnRate".Translate().Colorize(ColoredText.ThreatColor) + $": {rate:0.000}/sec\n";
        }

        if (IsDraggable) {
            if (gene.targetValue <= 0.0) {
                tooltip += "NeverConsumeVial".Translate(metal.LabelCap).ToString();
            } else {
                tooltip += (string)("ConsumeVialBelow".Translate(metal.LabelCap) + ": ") +
                           gene.PostProcessValue(gene.targetValue) + '%';
            }
        }

        if (!gene.def.resourceDescription.NullOrEmpty()) {
            tooltip += $"\n\n{gene.def.resourceDescription.Formatted(gene.pawn.Named("PAWN"))}";
        }

        return tooltip;
    }

    protected override void DrawHeader(Rect headerRect, ref bool mouseOverElement) {
        if (IsDraggable) {
            Metalborn metalborn = gene as Metalborn;
            if (metalborn != null) {
                headerRect.xMax -= (ABILITY_ICON_SIZE + ABILITY_ICON_PADDING) * metalborn.def.abilities.Count;
                int i = 0;
                metalborn.def.abilities.SortBy(x => x.uiOrder);
                foreach (AbilityDef abilityDef in metalborn.def.abilities) {
                    Ability ability = pawn.abilities.GetAbility(abilityDef);
                    Rect rect = new Rect(
                        headerRect.xMax + i * (ABILITY_ICON_SIZE + ABILITY_ICON_PADDING),
                        headerRect.y,
                        ABILITY_ICON_SIZE,
                        ABILITY_ICON_SIZE
                    );
                    Verse.Command? gizmo =
                        (Verse.Command)Activator.CreateInstance(abilityDef.gizmoClass, ability, pawn);
                    gizmo.GizmoOnGUIShrunk(
                        new Vector2(rect.xMin, rect.yMin),
                        ABILITY_ICON_SIZE,
                        new GizmoRenderParms()
                    );

                    if (Mouse.IsOver(rect)) {
                        Widgets.DrawHighlight(rect);
                        TooltipHandler.ClearTooltipsFrom(headerRect);
                        mouseOverElement = true;
                    }

                    i++;
                }
            }
        }

        base.DrawHeader(headerRect, ref mouseOverElement);
    }
}