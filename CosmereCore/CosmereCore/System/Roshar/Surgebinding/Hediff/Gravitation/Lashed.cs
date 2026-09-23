using Cosmere.Core.Ability;
using Cosmere.Core.Hediff;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Hediff.Gravitation;

public class Lashed : SurgebindingHediff {
    public Lashed() { }

    public Lashed(HediffDef hediffDef, Pawn pawn, IAbility<Surgebinder, IHediff<Surgebinder>> ability) : base(hediffDef, pawn, ability) { }
}
