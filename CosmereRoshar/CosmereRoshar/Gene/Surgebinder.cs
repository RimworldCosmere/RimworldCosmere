using Cosmere.Core.Gene;
using Cosmere.Core.Need;
using Cosmere.Roshar.Def;
using Cosmere.Roshar.DefModExtension;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.Gene;

public class Surgebinder : Invested {
    public int currentIdeal;
    public RadiantOrder radiantOrder => def.GetModExtension<RadiantOrder>();
    public RadiantOrderDef radiantOrderDef => radiantOrder.order;
    protected override Color BarColor => radiantOrderDef.gemstone.color.SaturationChanged(1f);
    protected override Color BarHighlightColor => radiantOrderDef.gemstone.color.SaturationChanged(2f);
    private Investiture investiture => pawn.needs.TryGetNeed<Investiture>();

    public override float Max => investiture.MaxLevel;
    public override float Value => investiture.CurLevel;

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref currentIdeal, "currentIdeal");
    }
}