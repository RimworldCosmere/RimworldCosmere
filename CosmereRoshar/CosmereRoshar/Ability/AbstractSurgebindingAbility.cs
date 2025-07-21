using Cosmere.Core.Ability;
using Cosmere.Resources.Def;
using Cosmere.Roshar.Def;
using Cosmere.Roshar.Extension;
using Cosmere.Roshar.Gene;
using Cosmere.Roshar.Hediff;
using RimWorld;
using Verse;

namespace Cosmere.Roshar.Ability;

public class AbstractSurgebindingAbility(Pawn pawn, AbilityDef def)
    : AbstractAbility<Surgebinder, SurgebinderHediff>(pawn, def) {
    public GemDef gem => def.gem;
    public override Surgebinder gene => cachedGene ??= pawn.genes.GetSurgebindingGeneForGem(gem)!;

    public new SurgebinderAbilityDef def {
        get => (SurgebinderAbilityDef)base.def;
        set => base.def = value;
    }

    public new bool GizmosVisible() {
        return base.GizmosVisible() && pawn.genes.HasSurgebindingGeneForGem(gem);
    }
}