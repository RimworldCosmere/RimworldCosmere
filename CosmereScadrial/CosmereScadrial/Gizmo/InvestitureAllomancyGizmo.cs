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
    private const float AbilityIconSize = 24;
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

    protected override IEnumerable<float> GetBarThresholds() {
        for (int i = 0; i < 100; i += 10) {
            yield return i;
        }
    }

    protected override void DrawHeader(Rect headerRect, ref bool mouseOverElement) {
        if (IsDraggable) {
            Metalborn metalborn = gene as Metalborn;
            if (metalborn != null) {
                headerRect.xMax -= AbilityIconSize * metalborn.def.abilities.Count;
                int i = 0;
                metalborn.def.abilities.SortBy(x => x.uiOrder);
                foreach (AbilityDef ability in metalborn.def.abilities) {
                    Rect rect = new Rect(headerRect.xMax + i * AbilityIconSize, headerRect.y, AbilityIconSize,
                        AbilityIconSize);
                    Verse.Command? gizmo = (Verse.Command)Activator.CreateInstance(ability.gizmoClass, ability, pawn);
                    gizmo.GizmoOnGUIShrunk(new Vector2(headerRect.xMin, headerRect.yMin), AbilityIconSize,
                        new GizmoRenderParms());
                    if (Mouse.IsOver(rect)) {
                        Widgets.DrawHighlight(rect);
                        TooltipHandler.TipRegion(rect, ability.description);
                        mouseOverElement = true;
                    }

                    i++;
                }
            }
        }

        base.DrawHeader(headerRect, ref mouseOverElement);
    }
}