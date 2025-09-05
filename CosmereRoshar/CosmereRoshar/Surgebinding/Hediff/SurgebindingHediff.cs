using Cosmere.Core.Ability;
using Cosmere.Core.Hediff;
using Cosmere.Roshar.Gene;
using Verse;

namespace Cosmere.Roshar.Surgebinding.Hediff;

public class SurgebindingHediff : AbstractHediff<Surgebinder> {
    public SurgebindingHediff() { }

    public SurgebindingHediff(HediffDef hediffDef, Pawn pawn, IAbility<Surgebinder, IHediff<Surgebinder>> ability) :
        base(hediffDef, pawn, ability) { }
}