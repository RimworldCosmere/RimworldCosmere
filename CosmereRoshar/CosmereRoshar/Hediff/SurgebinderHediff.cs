using Cosmere.Core.Ability;
using Cosmere.Core.Hediff;
using Cosmere.Roshar.Gene;
using Verse;

namespace Cosmere.Roshar.Hediff;

public class SurgebinderHediff(HediffDef hediffDef, Pawn pawn, IAbility<Surgebinder, IHediff<Surgebinder>> ability)
    : AbstractHediff<Surgebinder>(hediffDef, pawn, ability);