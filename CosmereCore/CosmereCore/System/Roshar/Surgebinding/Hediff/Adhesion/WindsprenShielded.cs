using Cosmere.Core.Ability;
using Cosmere.Core.Hediff;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Hediff.Adhesion;

public class WindsprenShielded : SurgebindingHediff {
    public WindsprenShielded() { }

    public WindsprenShielded(HediffDef hediffDef, Pawn pawn, IAbility<Surgebinder, IHediff<Surgebinder>> ability) : base(hediffDef, pawn, ability) { }
}
