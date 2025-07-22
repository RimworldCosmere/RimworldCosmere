using Cosmere.Core.Ability;
using Cosmere.Core.Hediff;
using Cosmere.Roshar.Gene;
using Verse;

namespace Cosmere.Roshar.Surgebinding.Hediff;

public class SurgebindingHediff(HediffDef hediffDef, Pawn pawn, IAbility<Surgebinder, IHediff<Surgebinder>> ability)
    : AbstractHediff<Surgebinder>(hediffDef, pawn, ability);