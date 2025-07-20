using System.Collections.Generic;
using Cosmere.Core.Gizmo;
using Cosmere.Roshar.Def;
using Cosmere.Roshar.DefModExtension;
using Cosmere.Roshar.Gene;
using RimWorld;
using UnityEngine;

namespace Cosmere.Roshar.Gizmo;

public class RadiantOrderCommand(
    Gene_Resource gene,
    List<IGeneResourceDrain> drainGenes,
    Color barColor,
    Color barHighlightColor
) : CosmereGeneCommand<AbilitySubGizmo, Surgebinder>(gene, drainGenes, barColor, barHighlightColor) {
    private RadiantOrder radiantOrder => gene.def.GetModExtension<RadiantOrder>();
    private RadiantOrderDef radiantOrderDef => radiantOrder.order;

    protected override Texture2D GetIcon() {
        return gene.def.Icon;
    }

    /**
     * Loop through the radiantOrderDef, based on the gene.currentIdeal, and create the abilities and build the gizmos
     */
    protected override IEnumerable<AbilitySubGizmo> GetSubGizmos() {
        return [];
    }
}