using Cosmere.System.Scadrial.Util;
using Verse;

namespace Cosmere.System.Scadrial.Gene;

/// <summary>
///     Starts the growth clock, however the koloss came to exist.
/// </summary>
/// <remarks>
///     Until now only the surgery added <c>Cosmere_Scadrial_Hediff_KolossGrowth</c>, so a koloss
///     from a raid, the dev xenotype menu or a post-Catacendre birth had no clock at all - it was
///     a big blue person that would live forever. Hanging it on the gene covers every path at
///     once: <c>Pawn_GeneTracker.AddGene</c> calls <c>PostAdd</c>, and
///     <c>PawnGenerator.GenerateGenes</c> reaches it through <c>SetXenotype</c>.
/// </remarks>
public class KolossHeritage : Verse.Gene {
    public override void PostAdd() {
        // Not empty on the base: it dirties the graphics when the gene defines any.
        base.PostAdd();

        if (pawn.health?.hediffSet == null) return;

        HediffDef? growth = HediffDefOf.Cosmere_Scadrial_Hediff_KolossGrowth;
        if (growth == null) return;
        if (pawn.health.hediffSet.GetFirstHediffOfDef(growth) != null) return;

        pawn.health.AddHediff(growth);
    }
}
