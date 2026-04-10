using Cosmere.Core.Ability;
using Cosmere.Core.Hediff;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Hediff.Tension;

public class Rigidized : SurgebindingHediff {
    public Rigidized() { }

    public Rigidized(HediffDef hediffDef, Pawn pawn, IAbility<Surgebinder, IHediff<Surgebinder>> ability) :
        base(hediffDef, pawn, ability) { }
}
