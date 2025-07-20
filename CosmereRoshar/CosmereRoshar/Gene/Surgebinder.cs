using Cosmere.Core.Gene;
using Cosmere.Roshar.Def;
using Cosmere.Roshar.DefModExtension;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.Gene;

public class Surgebinder : Invested {
    public int currentIdeal = 0;
    public RadiantOrder radiantOrder => def.GetModExtension<RadiantOrder>();
    public RadiantOrderDef radiantOrderDef => radiantOrder.order;
    protected override Color BarColor => radiantOrderDef.gemstone.color.SaturationChanged(1f);
    protected override Color BarHighlightColor => radiantOrderDef.gemstone.color.SaturationChanged(2f);

    public override float Max => 1f;
    public override float Value => 0.3f;
}