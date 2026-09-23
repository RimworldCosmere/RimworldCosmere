using Verse;

namespace Cosmere.Core.Hediff;

/// <summary>
///     A hediff whose def description is written about the pawn carrying it.
/// </summary>
/// <remarks>
///     HediffDef.description is shown verbatim on the health card - vanilla never puts grammar
///     placeholders in one, and none of the 379 in Core do. Ours read better naming the pawn, so
///     resolve them here rather than flattening the prose.
/// </remarks>
public class PawnDescribedHediff : HediffWithComps {
    public override string Description => base.Description.Formatted(pawn.Named("PAWN")).Resolve();
}
