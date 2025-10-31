using Cosmere;
using Cosmere.Core.Ability;
using Cosmere.Def;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Surgebinding.Hediff;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability;

public class SurgebindingAbility : AbstractAbility<Surgebinder, SurgebindingHediff> {
    public SurgebindingAbility(Pawn pawn) : base(pawn) { }
    public SurgebindingAbility(Pawn pawn, AbilityDef def) : base(pawn, def) { }

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

    public override float GetDesiredBurnRateForStatus(Status? desiredStatus) {
        return base.GetDesiredBurnRateForStatus(desiredStatus) / (gene.currentIdeal + 1);
    }
}