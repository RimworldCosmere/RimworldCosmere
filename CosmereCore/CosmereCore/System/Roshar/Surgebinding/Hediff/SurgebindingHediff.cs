using Cosmere.Core.Ability;
using Cosmere.Core.Hediff;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Hediff;

public class SurgebindingHediff : AbstractHediff<Surgebinder> {
    public SurgebindingHediff() { }

    public SurgebindingHediff(HediffDef hediffDef, Pawn pawn, IAbility<Surgebinder, IHediff<Surgebinder>> ability) :
        base(hediffDef, pawn, ability) { }
}