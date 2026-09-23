using Cosmere.Core.Ability;
using Cosmere.Core.Hediff;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Hediff.Abrasion;

public class Slicked : SurgebindingHediff {
    public Slicked() { }

    public Slicked(HediffDef hediffDef, Pawn pawn, IAbility<Surgebinder, IHediff<Surgebinder>> ability) : base(hediffDef, pawn, ability) { }
}
