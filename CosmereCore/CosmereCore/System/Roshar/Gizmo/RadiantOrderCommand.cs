using System;
using System.Text;
using Cosmere.Core.Gizmo;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.DefModExtension;
using Cosmere.System.Roshar.Dialog;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Surgebinding.Ability;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Gizmo;

public class RadiantOrderCommand(
    Gene_Resource gene,
    List<IGeneResourceDrain> drainGenes,
    Color barColor,
    Color barHighlightColor
) : CosmereGeneCommand<SurgebindingAbilitySubGizmo, Surgebinder>(gene, drainGenes, barColor, barHighlightColor) {
    private RadiantOrder radiantOrder =>
        gene.def.GetModExtension<RadiantOrder>() ??
        throw new InvalidOperationException($"GeneDef '{gene.def.defName}' is missing RadiantOrder mod extension");
    private RadiantOrderDef radiantOrderDef => radiantOrder.order;
    protected override bool useResourceLabelForTitle => false;

    private int orderAbilityCount => GetSubGizmos().Count();

    protected override float Width {
        get {
            if (shrunk) return Height + Padding.x / 2;
            return GetWidthForAbilityCount(Math.Max(2, orderAbilityCount));
        }
    }

    protected override Texture2D GetIcon() {
        return gene.def.Icon;
    }

    protected override string GetTooltipHeader() {
        StringBuilder sb = new StringBuilder(base.GetTooltipHeader() + "\n");

        TaggedString ideal = $"CC_Ordinal_{gene.CurrentIdealDisplay}_Long".Translate() +
                             ' ' +
                             "CRO_RadiantOrder_Ideal".Translate();

        sb.AppendLine(ideal.Resolve().Colorize(ColorLibrary.GrassGreen));

        return sb.ToString();
    }

    /**
     * Only show order-level abilities (base + ideal-specific), NOT surge abilities.
     * Surge abilities are shown in separate SurgeGizmo instances.
     */
    protected override IEnumerable<SurgebindingAbilitySubGizmo> GetSubGizmos() {
        HashSet<AbilityDef> shown = [];

        foreach (AbilityDef abilityDef in radiantOrderDef.abilities) {
            if (!pawn.TryGetAbility(abilityDef, out SurgebindingAbility? ability) || ability == null) continue;
            if (!ability.GizmosVisible()) continue;
            shown.Add(abilityDef);
            yield return new SurgebindingAbilitySubGizmo(this, gene, ability);
        }

        for (int i = 0; i <= Math.Min(gene.CurrentIdeal, radiantOrderDef.ideals.Count - 1); i++) {
            Ideal ideal = radiantOrderDef.ideals[i];
            foreach (AbilityDef idealAbility in ideal.abilities) {
                if (!pawn.TryGetAbility(idealAbility, out SurgebindingAbility? ability) || ability == null) continue;
                if (!ability.GizmosVisible()) continue;
                shown.Add(idealAbility);
                yield return new SurgebindingAbilitySubGizmo(this, gene, ability);
            }
        }

        List<Ability> allAbilities = pawn.abilities.abilities;
        for (int i = 0; i < allAbilities.Count; i++) {
            if (allAbilities[i] is not SurgebindingAbility sa) continue;
            if (shown.Contains(sa.def)) continue;
            if (sa.def is not SurgebindingAbilityDef sad || sad.radiantOrder != radiantOrderDef) continue;
            if (!sa.GizmosVisible()) continue;
            yield return new SurgebindingAbilitySubGizmo(this, gene, sa);
        }
    }

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms) {
        float btnSize = 18f;
        Rect infoRect = new Rect(
            topLeft.x + 4f,
            topLeft.y + 10f,
            btnSize,
            btnSize
        );

        bool infoClicked = Mouse.IsOver(infoRect) &&
                           Event.current.type == EventType.MouseDown &&
                           Event.current.button == 0;

        if (infoClicked) {
            Event.current.Use();
            Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
            if (surgebinder != null) {
                Find.WindowStack.Add(
                    new Dialog_RadiantOrderInfoDialog(
                        pawn,
                        surgebinder,
                        RadiantOrderInfoMode.View
                    )
                );
            }
        }

        Surgebinder? surgebinderGene = gene;
        bool sprenClicked = false;
        Rect sprenRect = default;
        if (surgebinderGene?.bondedSpren != null) {
            SprenBond? sprenBond = surgebinderGene.bondedSpren.TryGetComp<SprenBond>();
            if (sprenBond != null) {
                sprenRect = new Rect(
                    infoRect.xMax + 2f,
                    infoRect.y,
                    btnSize,
                    btnSize
                );

                sprenClicked = Mouse.IsOver(sprenRect) &&
                               Event.current.type == EventType.MouseDown &&
                               Event.current.button == 0;

                if (sprenClicked) {
                    Event.current.Use();
                    if (Event.current.shift) {
                        sprenBond.ToggleAutonomy();
                    }
                    else if (!sprenBond.CooldownActive) {
                        if (sprenBond.Dismissed) {
                            if (pawn.Map != null) {
                                sprenBond.Summon(pawn.Map, pawn.Position);
                            }
                        }
                        else {
                            sprenBond.Dismiss();
                        }
                    }
                }
            }
        }

        GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);

        GUI.DrawTexture(infoRect, TexButton.Info);
        if (Mouse.IsOver(infoRect)) {
            Widgets.DrawHighlight(infoRect);
            TooltipHandler.TipRegion(
                infoRect,
                new TipSignal("CRO_RadiantOrder_InfoButton".Translate(), Gen.HashCombineInt(GetHashCode(), 8491284))
            );
        }

        if (surgebinderGene?.bondedSpren != null) {
            SprenBond? sprenBond = surgebinderGene.bondedSpren.TryGetComp<SprenBond>();
            if (sprenBond != null) {
                string texPath = surgebinderGene.bondedSpren.kindDef.lifeStages.Last().bodyGraphicData.texPath;
                Texture2D sprenIcon = ContentFinder<Texture2D>.Get(texPath, false) ?? BaseContent.BadTex;
                GUI.DrawTexture(sprenRect, sprenIcon);

                string tooltip = sprenBond.Dismissed
                    ? "CRO_SprenSummon".Translate() + "\n" + "CRO_SprenSummon_Desc".Translate()
                    : "CRO_SprenDismiss".Translate() + "\n" + "CRO_SprenDismiss_Desc".Translate();
                if (sprenBond.Dismissed && pawn.Map == null) {
                    tooltip += "\n\n" + "CRO_SprenSummon_NoMap".Translate().Colorize(ColorLibrary.RedReadable);
                }

                if (sprenBond.CooldownActive) {
                    tooltip += "\n\n" + "CRO_SprenCooldown".Translate().Colorize(ColorLibrary.Yellow);
                }

                if (sprenBond.Autonomous) {
                    tooltip += "\n" + "CRO_SprenAutonomous_On".Translate().Colorize(ColorLibrary.Cyan);
                }

                tooltip += "\n\n" + "CRO_SprenAutonomous_Hint".Translate().Colorize(ColorLibrary.Grey);
                if (Mouse.IsOver(sprenRect)) {
                    Widgets.DrawHighlight(sprenRect);
                    TooltipHandler.TipRegion(
                        sprenRect,
                        new TipSignal(tooltip, Gen.HashCombineInt(GetHashCode(), 8491284))
                    );
                }
            }
        }

        return result;
    }
}