using System.Text;
using Cosmere.Core.Gizmo;
using Cosmere.Roshar.Def;
using Cosmere.Roshar.DefModExtension;
using Cosmere.Roshar.Gene;
using Cosmere.Roshar.Surgebinding.Ability;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.Gizmo;

public class RadiantOrderCommand(
    Gene_Resource gene,
    List<IGeneResourceDrain> drainGenes,
    Color barColor,
    Color barHighlightColor
) : CosmereGeneCommand<SurgebindingAbilitySubGizmo, Surgebinder>(gene, drainGenes, barColor, barHighlightColor) {
    private RadiantOrder radiantOrder => gene.def.GetModExtension<RadiantOrder>();
    private RadiantOrderDef radiantOrderDef => radiantOrder.order;
    protected override bool useResourceLabelForTitle => false;

    protected override Texture2D GetIcon() {
        return gene.def.Icon;
    }

    protected override string GetTooltipHeader() {
        StringBuilder sb = new StringBuilder(base.GetTooltipHeader() + "\n");

        TaggedString ideal = $"CF_Ordinal_{gene.currentIdealDisplay}_Long".Translate() +
                             ' ' +
                             "CRO_RadiantOrder_Ideal".Translate();

        sb.AppendLine(ideal.Resolve().Colorize(ColorLibrary.GrassGreen));

        return sb.ToString();
    }

    /**
     * Loop through the radiantOrderDef, based on the gene.currentIdeal, and create the abilities and build the gizmos
     */
    protected override IEnumerable<SurgebindingAbilitySubGizmo> GetSubGizmos() {
        foreach (AbilityDef abilityDef in radiantOrderDef.GetAbilities(gene.currentIdeal)) {
            if (!pawn.TryGetAbility(abilityDef, out SurgebindingAbility ability)) continue;
            if (!ability.GizmosVisible()) continue;

            yield return new SurgebindingAbilitySubGizmo(this, gene, ability);
        }
    }
}