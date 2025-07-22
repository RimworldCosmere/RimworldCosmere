using Cosmere.Core.Ability;
using Cosmere.Resources.Def;
using Cosmere.Roshar.Def;
using Cosmere.Roshar.Gene;
using Cosmere.Roshar.Surgebinding.Hediff;
using RimWorld;
using Verse;

namespace Cosmere.Roshar.Surgebinding.Ability;

public class SurgebindingAbility(Pawn pawn, AbilityDef def)
    : AbstractAbility<Surgebinder, SurgebindingHediff>(pawn, def) {
    public RadiantOrderDef radiantOrder => def.radiantOrder ?? pawn.GetRadiantOrder()!;
    public GemDef gem => radiantOrder.gemstone;

    public override Surgebinder gene => cachedGene ??= pawn.genes.GetSurgebindingGeneForOrder(radiantOrder)!;

    public new SurgebindingAbilityDef def {
        get => (SurgebindingAbilityDef)base.def;
        set => base.def = value;
    }

    public new bool GizmosVisible() {
        return base.GizmosVisible() && pawn.genes.HasSurgebindingGeneForOrder(radiantOrder);
    }
}