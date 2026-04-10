using Cosmere.Core.Ability;
using Cosmere.Core.Hediff;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Hediff.Gravitation;

public class GravitationallyShielded : SurgebindingHediff {
    public GravitationallyShielded() { }

    public GravitationallyShielded(HediffDef hediffDef, Pawn pawn,
        IAbility<Surgebinder, IHediff<Surgebinder>> ability) :
        base(hediffDef, pawn, ability) { }
}
